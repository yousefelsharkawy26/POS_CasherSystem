using System.IO;
using Microsoft.Win32;
using Wpf.Ui.Appearance;
using POS_ModernUI.Helpers;
using Microsoft.Data.SqlClient;
using System.Windows.Threading;
using Wpf.Ui.Abstractions.Controls;
using Microsoft.Extensions.Configuration;

namespace POS_ModernUI.ViewModels.Pages;

public partial class SettingsViewModel : ObservableObject, INavigationAware
{
    #region Fields
    private bool _isInitialized = false;
    private string _currentDomain;
    private readonly string _connectionString;
    private readonly string _databaseName = "CasherSystem_Db";
    private readonly string _backupFolder = @"D:\DbBackups";
    private readonly string _jobName = "POS_Daily_Backup";
    private readonly string _scheduleName = "POS_Schedule";
    private DispatcherTimer countdownTimer = new();
    private DateTime? nextScheduleTime;
    #endregion

    #region Props
    [ObservableProperty] private string _appVersion = string.Empty;
    [ObservableProperty] private ApplicationTheme _currentTheme = ApplicationTheme.Unknown;
    [ObservableProperty] private bool _isDarkTheme = false;
    [ObservableProperty]
    private List<string> _scheduleChoices = [
        "أبداً",
        "يومي",
        "أسبوعي",
        "شهري"
    ];
    [ObservableProperty] private string _selectedScheduleDomain;
    [ObservableProperty] private string? _countDown;
    #endregion

    #region Constructors
    public SettingsViewModel(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("Devconn")!;
        SelectedScheduleDomain = _currentDomain = CurrentScheduleDomain();

        // التأكد من وجود الـ Job عند التهيئة
        EnsureJobExists();
    }
    #endregion

    #region Initializations
    public Task OnNavigatedToAsync()
    {
        if (!_isInitialized)
            InitializeViewModel();

        return Task.CompletedTask;
    }

    public Task OnNavigatedFromAsync() => Task.CompletedTask;

    private void InitializeViewModel()
    {
        CurrentTheme = ApplicationThemeManager.GetAppTheme();
        IsDarkTheme = CurrentTheme == ApplicationTheme.Dark;
        AppVersion = $"POS Modern App - {GetAssemblyVersion()} By Elsharkawy";
        _isInitialized = true;
    }

    private string GetAssemblyVersion()
    {
        return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString()
            ?? string.Empty;
    }
    #endregion

    #region Commands
    [RelayCommand]
    private void OnChangeTheme(string parameter)
    {
        
        CurrentTheme = IsDarkTheme ? ApplicationTheme.Dark : ApplicationTheme.Light;
        ApplicationThemeManager.Apply(CurrentTheme);
    }

    [RelayCommand]
    private void OnSubmitChangeScheduleDomain()
    {
        var msg = new Wpf.Ui.Controls.MessageBox();
        if (SelectedScheduleDomain == _currentDomain)
        {
            msg.ShowMessage("الإختيار دا تم تطبيقه مسبقاً", "اختيار مشابه");
            return;
        }

        try
        {
            UpdateJobSchedule(SelectedScheduleDomain);
            _currentDomain = SelectedScheduleDomain;
            OnStartCountdown();
            msg.ShowMessage("تم تحديث جدولة النسخ الاحتياطي بنجاح وبدء العد التنازلي", "تم بنجاح");
        }
        catch (Exception ex)
        {
            msg.ShowMessage($"❌ حدث خطأ في تحديث الجدولة:\n{ex.Message}", "خطأ");
        }
    }

    [RelayCommand]
    private void OnManualBackup()
    {
        try
        {
            // التأكد من وجود المجلد
            if (!Directory.Exists(_backupFolder))
                Directory.CreateDirectory(_backupFolder);

            string fileName = $"{_databaseName}_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
            string fullPath = Path.Combine(_backupFolder, fileName);

            // SQL صحيح بدون semicolon في البداية
            string sql = $@"
                    BACKUP DATABASE [{_databaseName}]
                    TO DISK = N'{fullPath}'
                    WITH COPY_ONLY, INIT, COMPRESSION, CHECKSUM, STATS = 10,
                          NAME = N'{_databaseName} Manual Backup';";

            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var command = new SqlCommand(sql, connection);
            command.CommandTimeout = 600; // 10 minutes timeout للـ backup
            command.ExecuteNonQuery();

            new Wpf.Ui.Controls.MessageBox().ShowMessage(
                $"✅ تم إنشاء النسخة الاحتياطية بنجاح:\n{fullPath}", "نجاح");
        }
        catch (SqlException sqlEx)
        {
            new Wpf.Ui.Controls.MessageBox().ShowMessage(
                $"❌ خطأ في قاعدة البيانات:\n{sqlEx.Message}\nError Number: {sqlEx.Number}", "خطأ SQL");
        }
        catch (Exception ex)
        {
            new Wpf.Ui.Controls.MessageBox().ShowMessage(
                $"❌ حدث خطأ أثناء النسخ:\n{ex.Message}", "خطأ");
        }
    }

    [RelayCommand]
    private void OnRestoreBackup()
    {
        OpenFileDialog dialog = new()
        {
            Filter = "Backup Files (*.bak)|*.bak",
            Title = "اختر ملف النسخة الاحتياطية"
        };

        if (dialog.ShowDialog() == true)
        {
            RestoreBackup(dialog.FileName);
        }
    }

    [RelayCommand]
    private void OnStartCountdown()
    {
        nextScheduleTime = GetNextScheduleTime();

        if (nextScheduleTime == null)
        {
            CountDown = "❎ لم يتم تحديد وقت الجدولة.";
            return;
        }

        countdownTimer.Stop();
        countdownTimer.Tick -= CountdownTimer_Tick;
        countdownTimer.Tick += CountdownTimer_Tick;
        countdownTimer.Interval = TimeSpan.FromSeconds(1);
        countdownTimer.Start();
    }
    #endregion

    #region SQL Agent Job Management
    private void EnsureJobExists()
    {
        try
        {
            // تم تحديث السلسلة النصية SQL Agent Job
            // لضمان عملها بشكل صحيح في SQL Server
            string checkAndCreateJobSql = $@"
                    IF NOT EXISTS (SELECT 1 FROM msdb.dbo.sysjobs WHERE name = N'{_jobName}')
                    BEGIN
                        EXEC msdb.dbo.sp_add_job
                            @job_name = N'{_jobName}',
                            @enabled = 1,
                            @description = N'Automated POS Database Backup Job',
                            @category_name = N'Database Maintenance';

                        EXEC msdb.dbo.sp_add_jobstep
                            @job_name = N'{_jobName}',
                            @step_name = N'BackupStep',
                            @subsystem = N'TSQL',
                            @database_name = N'master',
                            @command = N'
                                -- تعريف المتغيرات اللازمة
                                DECLARE @BackupPath NVARCHAR(260);
                                DECLARE @FileName NVARCHAR(200);
                                DECLARE @FullPath NVARCHAR(520);
                        
                                -- تعيين مسار النسخ الاحتياطي من متغير C#
                                SET @BackupPath = N''{_backupFolder}'';
                        
                                -- إنشاء اسم ملف ديناميكي يحتوي على التاريخ والوقت
                                SET @FileName = N''{_databaseName}_''
                                    + CONVERT(NVARCHAR(8), GETDATE(), 112) + N''_''
                                    + REPLACE(CONVERT(NVARCHAR(8), GETDATE(), 108), '':'', '''') + N''.bak'';

                                -- دمج المسار واسم الملف للحصول على المسار الكامل
                                SET @FullPath = @BackupPath + N''\'' + @FileName;

                                -- التأكد من وجود المجلد
                                -- xp_cmdshell يجب أن يكون مفعّلاً في SQL Server
                                EXEC master.dbo.xp_create_subdir @BackupPath;

                                -- تنفيذ أمر النسخ الاحتياطي
                                BACKUP DATABASE [{_databaseName}]
                                TO DISK = @FullPath
                                WITH INIT, COMPRESSION, CHECKSUM, STATS = 10,
                                     NAME = N''{_databaseName} Scheduled Backup'';
                            ';
                
                        -- ربط السيرفر المحلي بالمهمة
                        EXEC msdb.dbo.sp_add_jobserver @job_name = N'{_jobName}';

                        PRINT N'Job {_jobName} created successfully';
                    END
                    ELSE
                    BEGIN
                        PRINT N'Job {_jobName} already exists';
                    END";

            ExecuteSqlOnMsdb(checkAndCreateJobSql);
        }
        catch (Exception ex)
        {
            // لا نريد أن يتوقف التطبيق إذا فشل إنشاء الـ Job
            System.Diagnostics.Debug.WriteLine($"Error creating job: {ex.Message}");
        }
    }

    private void UpdateJobSchedule(string frequency)
    {
        try
        {
            string today = DateTime.Now.ToString("yyyyMMdd");

            // حذف الجدولة السابقة
            string deleteOldSchedule = $@"
                    -- حذف الجدولة السابقة إن وجدت
                    IF EXISTS (SELECT 1 FROM msdb.dbo.sysjobschedules js
                               JOIN msdb.dbo.sysschedules s ON js.schedule_id = s.schedule_id
                               JOIN msdb.dbo.sysjobs j ON js.job_id = j.job_id
                               WHERE j.name = N'{_jobName}' AND s.name = N'{_scheduleName}')
                    BEGIN
                        EXEC msdb.dbo.sp_detach_schedule 
                            @job_name = N'{_jobName}', 
                            @schedule_name = N'{_scheduleName}';
    
                        EXEC msdb.dbo.sp_delete_schedule 
                            @schedule_name = N'{_scheduleName}';
        
                        PRINT N'Old schedule deleted';
                    END";

            string addNewSchedule = "";

            // إضافة الجدولة الجديدة حسب التكرار المطلوب
            switch (frequency)
            {
                case "يومي":
                    addNewSchedule = $@"
                            -- إضافة جدولة يومية
                            EXEC msdb.dbo.sp_add_schedule
                                @schedule_name = N'{_scheduleName}',
                                @enabled = 1,
                                @freq_type = 4,          -- يومي
                                @freq_interval = 1,      -- كل يوم
                                @freq_subday_type = 1,    -- في وقت محدد
                                @freq_recurrence_factor = 1,
                                @active_start_date = {today},
                                @active_start_time = 020000;  -- 2:00 AM

                            EXEC msdb.dbo.sp_attach_schedule
                                @job_name = N'{_jobName}',
                                @schedule_name = N'{_scheduleName}';
    
                            PRINT N'Daily schedule added';";
                    break;

                case "أسبوعي":
                    addNewSchedule = $@"
                            -- إضافة جدولة أسبوعية (كل يوم اثنين)
                            EXEC msdb.dbo.sp_add_schedule
                                @schedule_name = N'{_scheduleName}',
                                @enabled = 1,
                                @freq_type = 8,          -- أسبوعي
                                @freq_interval = 2,      -- الاثنين (1=Sunday, 2=Monday, etc.)
                                @freq_subday_type = 1,    -- في وقت محدد
                                @freq_recurrence_factor = 1, -- كل أسبوع
                                @active_start_date = {today},
                                @active_start_time = 020000;  -- 2:00 AM

                            EXEC msdb.dbo.sp_attach_schedule
                                @job_name = N'{_jobName}',
                                @schedule_name = N'{_scheduleName}';
    
                            PRINT N'Weekly schedule added';";
                    break;

                case "شهري":
                    addNewSchedule = $@"
                            -- إضافة جدولة شهرية (أول يوم في الشهر)
                            EXEC msdb.dbo.sp_add_schedule
                                @schedule_name = N'{_scheduleName}',
                                @enabled = 1,
                                @freq_type = 16,         -- شهري
                                @freq_interval = 1,      -- اليوم الأول من الشهر
                                @freq_subday_type = 1,    -- في وقت محدد
                                @freq_recurrence_factor = 1, -- كل شهر
                                @active_start_date = {today},
                                @active_start_time = 020000;  -- 2:00 AM

                            EXEC msdb.dbo.sp_attach_schedule
                                @job_name = N'{_jobName}',
                                @schedule_name = N'{_scheduleName}';
    
                            PRINT N'Monthly schedule added';";
                    break;

                case "أبداً":
                    // لا نضيف أي جدولة، فقط نحذف الموجودة
                    break;
            }

            // تنفيذ SQL commands
            string fullSql = deleteOldSchedule;
            if (!string.IsNullOrEmpty(addNewSchedule))
            {
                fullSql += Environment.NewLine + addNewSchedule;
            }

            ExecuteSqlOnMsdb(fullSql);
        }
        catch (Exception ex)
        {
            throw new Exception($"فشل في تحديث جدولة النسخ الاحتياطي: {ex.Message}");
        }
    }

    private void ExecuteSqlOnMsdb(string sql)
    {
        // إنشاء connection string للـ msdb
        var builder = new SqlConnectionStringBuilder(_connectionString)
        {
            InitialCatalog = "msdb"
        };

        using var connection = new SqlConnection(builder.ConnectionString);
        connection.Open();

        // تقسيم الـ SQL إلى statements منفصلة لتجنب مشاكل الـ GO
        var statements = sql.Split(new[] { "\r\nGO\r\n", "\nGO\n", "\rGO\r" },
            StringSplitOptions.RemoveEmptyEntries);

        foreach (var statement in statements)
        {
            if (!string.IsNullOrWhiteSpace(statement))
            {
                using var command = new SqlCommand(statement.Trim(), connection);
                command.CommandTimeout = 120; // 2 minutes timeout
                command.ExecuteNonQuery();
            }
        }
    }
    #endregion

    #region Schedule Information
    private string CurrentScheduleDomain()
    {
        try
        {
            string sql = @"
                    SELECT 
                        CASE s.freq_type
                            WHEN 4  THEN N'يومي'
                            WHEN 8  THEN N'أسبوعي'
                            WHEN 16 THEN N'شهري'
                            ELSE N'غير معروف'
                        END AS ScheduleType
                    FROM msdb.dbo.sysjobs j
                    JOIN msdb.dbo.sysjobschedules js ON j.job_id = js.job_id
                    JOIN msdb.dbo.sysschedules s ON js.schedule_id = s.schedule_id
                    WHERE j.name = @JobName AND s.enabled = 1";

            var builder = new SqlConnectionStringBuilder(_connectionString) { InitialCatalog = "msdb" };

            using var connection = new SqlConnection(builder.ConnectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@JobName", _jobName);
            connection.Open();

            var result = command.ExecuteScalar()?.ToString();

            return (result == "يومي" || result == "أسبوعي" || result == "شهري") ? result! : "أبداً";
        }
        catch
        {
            return "أبداً";
        }
    }
    private DateTime? GetNextScheduleTime()
    {
        try
        {
            const string sql = @"
                    SELECT s.freq_type, s.freq_interval, s.active_start_time
                    FROM msdb.dbo.sysjobs j
                    JOIN msdb.dbo.sysjobschedules js ON j.job_id = js.job_id
                    JOIN msdb.dbo.sysschedules s ON js.schedule_id = s.schedule_id
                    WHERE j.name = @JobName AND s.enabled = 1";

            var builder = new SqlConnectionStringBuilder(_connectionString) { InitialCatalog = "msdb" };

            using var connection = new SqlConnection(builder.ConnectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@JobName", _jobName);
            connection.Open();

            using var reader = command.ExecuteReader();
            if (!reader.Read())
                return null;

            int Ord(string name) => reader.GetOrdinal(name);
            int freqType = reader.GetInt32(Ord("freq_type"));
            int freqInterval = reader.GetInt32(Ord("freq_interval"));   // weekly: bitmask
            int timeInt = reader.GetInt32(Ord("active_start_time"));

            int hour = timeInt / 10000;
            int minute = (timeInt % 10000) / 100;
            int second = timeInt % 100;

            DateTime now = DateTime.Now;
            DateTime todayAt = now.Date.AddHours(hour).AddMinutes(minute).AddSeconds(second);

            switch (freqType)
            {
                case 4: // Daily
                    return (todayAt > now) ? todayAt : todayAt.AddDays(1);

                case 8: // Weekly (bitmask: Sun=1, Mon=2, Tue=4, Wed=8, Thu=16, Fri=32, Sat=64)
                    {
                        int mask = freqInterval;

                        // ابحث عن أول يوم مُفعل بعد الآن خلال الأسبوع الحالي
                        for (int offset = 0; offset < 7; offset++)
                        {
                            var candidate = todayAt.AddDays(offset);
                            int bit = 1 << (int)candidate.DayOfWeek; // Sunday=0 -> 1
                            if ((mask & bit) != 0 && candidate > now)
                                return candidate;
                        }

                        // لو مفيش يوم متبقّي هذا الأسبوع، رجّع أول يوم مُفعل في الأسبوع القادم
                        for (int offset = 1; offset <= 7; offset++)
                        {
                            var candidate = todayAt.AddDays(offset);
                            int bit = 1 << (int)candidate.DayOfWeek;
                            if ((mask & bit) != 0)
                                return candidate;
                        }

                        return null;
                    }

                case 16: // Monthly (freq_interval = day-of-month)
                    {
                        int dom = Math.Max(1, Math.Min(31, freqInterval));
                        int daysThisMonth = DateTime.DaysInMonth(now.Year, now.Month);
                        int day = Math.Min(dom, daysThisMonth);

                        DateTime thisMonth = new DateTime(now.Year, now.Month, day, hour, minute, second);
                        if (thisMonth > now) return thisMonth;

                        var next = now.AddMonths(1);
                        int daysNext = DateTime.DaysInMonth(next.Year, next.Month);
                        day = Math.Min(dom, daysNext);
                        return new DateTime(next.Year, next.Month, day, hour, minute, second);
                    }

                default:
                    return null;
            }
        }
        catch
        {
            return null;
        }
    }
    #endregion

    #region Timer and UI Updates
    private void CountdownTimer_Tick(object? sender, EventArgs e)
    {
        if (nextScheduleTime == null) return;

        TimeSpan remaining = nextScheduleTime.Value - DateTime.Now;

        if (remaining <= TimeSpan.Zero)
        {
            countdownTimer.Stop();
            CountDown = "✅ تم الوصول إلى وقت الجدولة";

            // إعادة حساب الوقت التالي
            Task.Run(() =>
            {
                Thread.Sleep(2000); // انتظار ثانيتين
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnStartCountdown();
                });
            });
        }
        else
        {
            string timeDisplay = "";
            if (remaining.Days > 0)
                timeDisplay += $"{remaining.Days} يوم، ";

            timeDisplay += $"{remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";

            CountDown = $"⏰ الوقت المتبقي: {timeDisplay}";
        }
    }
    #endregion

    #region Backup Restoration
    private void RestoreBackup(string backupFilePath)
    {
        if (!File.Exists(backupFilePath))
        {
            new Wpf.Ui.Controls.MessageBox().ShowMessage("❌ ملف النسخة الاحتياطية غير موجود.", "خطأ");
            return;
        }

        // استخدام connection string للـ master database
        var builder = new SqlConnectionStringBuilder(_connectionString)
        {
            InitialCatalog = "master"
        };

        string sql = $@"
                -- إنهاء جميع الاتصالات للقاعدة
                ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

                -- استرجاع النسخة الاحتياطية
                RESTORE DATABASE [{_databaseName}]
                FROM DISK = N'{backupFilePath}'
                WITH REPLACE;

                -- إعادة تشغيل وضع المستخدمين المتعددين
                ALTER DATABASE [{_databaseName}] SET MULTI_USER;";

        try
        {
            using var connection = new SqlConnection(builder.ConnectionString);
            connection.Open();
            using var command = new SqlCommand(sql, connection);
            command.CommandTimeout = 600; // 10 minutes timeout
            command.ExecuteNonQuery();

            new Wpf.Ui.Controls.MessageBox().ShowMessage("✅ تم استرجاع النسخة الاحتياطية بنجاح", "تم");
        }
        catch (SqlException sqlEx)
        {
            new Wpf.Ui.Controls.MessageBox().ShowMessage(
                $"❌ خطأ في استرجاع قاعدة البيانات:\n{sqlEx.Message}\nError Number: {sqlEx.Number}", "خطأ SQL");
        }
        catch (Exception ex)
        {
            new Wpf.Ui.Controls.MessageBox().ShowMessage($"❌ حدث خطأ أثناء الاسترجاع:\n{ex.Message}", "خطأ");
        }
    }
    #endregion
}