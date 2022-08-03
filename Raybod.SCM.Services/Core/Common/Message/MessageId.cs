
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.Services.Core.Common.Message
{
    /// <summary>
    /// لیست خطاهایی که ر لایه سرویس به عنوان شرح پیام ارسال خواهد شد
    /// </summary>
    public enum MessageId
    {
        [Description("")]
        None = 0,
        /// <summary>
        ///عملیات با موفقیت انجام شد
        /// </summary>
        [Description("عملیات با موفقیت انجام شد.")]
        [Display(Name= "Operation completed successfully")]
        Succeeded = 1,
        /// <summary>
        ///خطای شماره 1 (لطفا با پشتیبانی تماس بگیرید)
        /// </summary>
        [Description("خطای شماره 1 (لطفا با پشتیبانی تماس بگیرید)")]
        [Display(Name = "Exception No. 1 ( please contact support)")]
        Exception = 3,

        /// <summary>
        /// خطایی در پردازش پیش آمده است
        /// </summary>
        [Description("خطایی در پردازش پیش آمده است")]
        [Display(Name = "خطایی در پردازش پیش آمده است")]
        InternalError = 4,

        /// <summary>
        /// ناسازگاری داده ای رخ داده است
        /// </summary>
        [Description("ناسازگاری داده ای رخ داده است")]
        [Display(Name = "Data consistency")]
        DataInconsistency = 5,

        /// <summary>
        /// موجودیت مورد نظر یافت نشد
        /// </summary>
        [Description("اطلاعات مورد نظر یافت نشد")]
        [Display(Name = "No information found")]
        EntityDoesNotExist = 6,



        /// <summary>
        ///اطلاعات ورودی معتبر نیستند
        /// </summary>
        [Description("اطلاعات ورودی معتبر نیستند")]
        [Display(Name = "Invalid information in one or more fields")]
        InputDataValidationError = 8,

        /// <summary>
        ///دسترسی غیر مجاز
        /// </summary>
        [Description("دسترسی غیر مجاز")]
        [Display(Name = "Access denied")]
        AccessDenied = 9,

        /// <summary>
        ///به دلیل وجود وابستگی به این موجودیت، امکان حذف آن وجود ندارد، ابتدا نسبت به حذف وابسته های آن اقدام کنید
        /// </summary>
        [Description("به دلیل وجود وابستگی به این موجودیت، امکان حذف آن وجود ندارد، ابتدا نسبت به حذف وابسته های آن اقدام کنید")]
        [Display(Name = "Removing information is not allowed due to having dependecies, first you need to remove all of them")]
        DeleteDontAllowedBeforeSubset = 10,

        /// <summary>
        ///کلمه عبور مطابقت ندارد  
        /// </summary>
        [Description("کلمه عبور مطابقت ندارد")]
        [Display(Name = "Password does not match")]
        PasswordNotMatch = 11,

        /// <summary>
        /// در درج موجودیت با خطا مواجه شد. 
        /// </summary>
        [Description(" ثبت اطلاعات با خطا مواجه شد.")]
        [Display(Name = "Registration failed")]
        AddEntityFailed = 12,

        /// <summary>
        /// در ویرایش  موجودیت با خطا مواجه شد. 
        /// </summary>
        [Description("ویرایش اطلاعات با خطا مواجه شد")]
        [Display(Name = "Saving information failed")]
        EditEntityFailed = 13,

        /// <summary>
        /// در حذف  موجودیت با خطا مواجه شد. 
        /// </summary>
        [Description("حذف اطلاعات با خطا مواجه شد")]
        [Display(Name = "Removing information  failed")]
        DeleteEntityFailed = 14,

        /// <summary>
        /// پارامترهای ارسالی ناقص می باشند
        /// </summary>
        [Description("پارامترهای ارسالی ناقص می باشند")]
        [Display(Name = "عملیات با موفقیت انجام شد.")]
        RequedredParameter = 15,

        [Description("رمز عبور فعلی اشتباه وارد شده است.")]
        [Display(Name = "The current password you have entered is incorrect")]
        OldPasswordNotCorect = 16,

        /// <summary>
        ///  دسترسی غیر مجاز، لطفا مجددا ورود کنید
        /// </summary>
        [Description("دسترسی غیر مجاز، لطفا مجددا ورود کنید.")]
        [Display(Name = "دسترسی غیر مجاز، لطفا مجددا ورود کنید.")]
        UserNotFound = 17,

        /// <summary>
        /// اطلاعات ورود نادرست است
        /// </summary>
        [Description("کاربری با این نام وجود ندارد.")]
        [Display(Name = "No user with this username was found")]
        SigninFailed = 18,

        /// <summary>
        /// کاربر باید یک یا بیش از یک نقش داشته باشد
        /// </summary>
        [Description("کاربر باید یک یا بیش از یک نقش داشته باشد")]
        [Display(Name = "کاربر باید یک یا بیش از یک نقش داشته باشد.")]
        UserMustBeHaveMoreThanZeroRole = 19,

        /// <summary>
        /// شماره تلفن قبلا ثبت شده است
        /// </summary>
        [Description("شماره تلفن قبلا ثبت شده است.")]
        [Display(Name = "شماره تلفن قبلا ثبت شده است.")]
        MobileExist = 20,

        /// <summary>
        /// اطلاعات درخواستی برای احراز هویت نادرست می باشد
        /// </summary>
        [Description("اطلاعات درخواستی برای احراز هویت نادرست می باشد")]
        [Display(Name = "اطلاعات درخواستی برای احراز هویت نادرست می باشد.")]
        TokenNotValid = 21,

        /// <summary>
        /// پارامتر های ارسالی صحیح نمی باشند
        /// </summary>
        [Description("اطلاعات ارسالی صحیح نمی باشند")]
        [Display(Name = "Submitted information is not valid")]
        ModelStateInvalid = 22,

        /// <summary>
        ///کاربر تا کنون ثبت نام نکرده است، لطفا ابتدا ثبت نام نمایید
        /// </summary>
        [Description("کاربر تا کنون ثبت نام نکرده است، لطفا ابتدا ثبت نام نمایید")]
        [Display(Name = "کاربر تا کنون ثبت نام نکرده است، لطفا ابتدا ثبت نام نمایید.")]
        UserNoteRegistered = 23,

        /// <summary>
        ///دسترسی شما از طرف شرکت قطع شده است لطفا با پشتیبان تماس بگیرید.
        /// </summary>
        [Description("دسترسی شما از طرف شرکت قطع شده است لطفا با پشتیبان تماس بگیرید.")]
        [Display(Name = "دسترسی شما از طرف شرکت قطع شده است لطفا با پشتیبان تماس بگیرید.")]
        UserIsDeActive = 24,

        /// <summary>
        /// ایمیل وارد شده صحیح نیست.
        /// </summary>
        [Description("ایمیل وارد شده صحیح نیست.")]
        [Display(Name = "Invalid email address")]
        EmailNotCorrect = 25,

        /// <summary>
        /// ذخیره سازی با خطا مواجه شد.
        /// </summary>
        [Description("ذخیره سازی با خطا مواجه شد.")]
        [Display(Name = "Saving information failed")]
        SaveFailed = 26,

        /// <summary>
        /// ارسال اعلان با خطا مواجه شد
        /// </summary>
        [Description("ارسال اعلان با خطا مواجه شد")]
        [Display(Name = "ارسال اعلان با خطا مواجه شد.")]
        SendNotificationFailed = 27,

        /// <summary>
        /// تکراری است.
        /// </summary>
        [Description("تکراری است.")]
        [Display(Name = "تکراری است.")]
        Duplicate = 28,

        /// <summary>
        /// ارسال اعلان با خطا مواجه شد
        /// </summary>
        [Description("ارسال اعلان با خطا مواجه شد")]
        [Display(Name = "Notification record not saved")]
        NotificationStateNotSaved = 29,

        /// <summary>
        ///  اعلان ارسالی به هیچ یک از کاربران ارسال نشده است.
        /// </summary>
        [Description("اعلان ارسالی به هیچ یک از کاربران ارسال نشده است.")]
        [Display(Name = "اعلان ارسالی به هیچ یک از کاربران ارسال نشده است.")]
        SendNotificationFailedByOneSignal = 30,

        /// <summary>
        /// کاربر وجود ندارد
        /// </summary>
        [Description("کاربر وجود ندارد.")]
        [Display(Name = "User not exist")]
        UserNotExist = 31,

        /// <summary>
        /// ایمیل قبلا استفاده شده است
        /// </summary>
        [Description("ایمیل قبلا استفاده شده است.")]
        [Display(Name = "ایمیل قبلا استفاده شده است.")]
        EmailExist = 32,

        /// <summary>
        /// 
        /// </summary>
        [Description("نام کاربری تکراری است")]
        [Display(Name = "نام کاربری تکراری است.")]
        UserNameExist = 33,



        /// <summary>
        /// کد تکراری می باشد
        /// </summary>
        [Description("کد تکراری می باشد")]
        [Display(Name = "The code you enterd is exsit already")]
        CodeExist = 35,

        /// <summary>
        /// حداکثر سقف سرگروه 9 تا می باشد
        /// </summary>
        [Description("حداکثر سقف سرگروه 9 تا می باشد")]
        [Display(Name = "حداکثر سقف سرگروه 9 تا می باشد.")]
        MaximumHeaderCategory = 36,

        /// <summary>
        /// حداکثر سقف زیر گروه ها 99 تا می باشد
        /// </summary>
        [Description("حداکثر سقف زیر گروه ها 99 تا می باشد")]
        [Display(Name = "حداکثر سقف زیر گروه ها 99 تا می باشد.")]
        MaximumSubCategory = 37,

        /// <summary>
        /// حداکثر تا چهار لایه زیرگروه مجاز می باشید
        /// </summary>
        [Description("حداکثر تا چهار لایه زیرگروه مجاز می باشید")]
        [Display(Name = "حداکثر تا چهار لایه زیرگروه مجاز می باشید.")]
        MaximumSubGroupLayer = 38,

        /// <summary>
        /// نام کاربری و پسورد خود را وارد کنید
        /// </summary>
        [Description("نام کاربری و پسورد خود را وارد کنید")]
        [Display(Name = "نام کاربری و پسورد خود را وارد کنید.")]
        UserNameOrPasswordNull = 39,

        /// <summary>
        /// مشتری وجود ندارد
        /// </summary>
        [Description("مشتری یافت نشد")]
        [Display(Name = "Custormer not found")]
        CustomerNotFound = 40,

        /// <summary>
        /// قرارداد یافت نشد  
        /// </summary>
        [Description("پروژه یافت نشد")]
        [Display(Name = "Project not found")]
        ContractNotFound = 41,
        /// <summary>
        /// سفارش یافت نشد
        /// </summary>
        [Description("سفارش یافت نشد")]
        [Display(Name = "سفارش یافت نشد")]
        ContractOrderNotFound = 42,

        /// <summary>
        /// سقف میزان سفارش رعایت نشده است
        /// </summary>
        [Description("سقف میزان سفارش رعایت نشده است")]
        [Display(Name = "سقف میزان سفارش رعایت نشده است")]
        MaximumQuntity = 43,

        /// <summary>
        /// شماره مدرک تکراری می باشد
        /// </summary>
        [Description("شماره مدرک تکراری می باشد")]
        [Display(Name = "Document number already exists")]
        DuplicatDocNumber = 44,

        /// <summary>
        /// شماره بازنگری تکراری می باشد
        /// </summary>
        [Description("شماره بازنگری تکراری می باشد")]
        [Display(Name = "شماره بازنگری تکراری می باشد")]
        DuplicatRevisionNumber = 45,

        /// <summary>
        /// بارگزاری فایل با خطا مواجه شده است
        /// </summary>
        [Description("بارگزاری فایل با خطا مواجه شده است")]
        [Display(Name = "File uploading failed")]
        UploudFailed = 46,

        /// <summary>
        /// سقف میزان خرید بر اساس برنامه ریزی ذعایت نشده است
        /// </summary>
        [Description("سقف میزان خرید بر اساس برنامه ریزی ذعایت نشده است")]
        [Display(Name = "سقف میزان خرید بر اساس برنامه ریزی ذعایت نشده است")]
        MaximumMrpPlanningQuntity = 47,

        /// <summary>
        /// بدلیل صدور RFP  امکان ویرایش وجود ندارد
        /// </summary>
        [Description("بدلیل صدور RFP  امکان ویرایش وجود ندارد")]
        [Display(Name = "بدلیل صدور RFP  امکان ویرایش وجود ندارد")]
        RequestProposalTypeChangeNotExist = 48,

        /// <summary>
        /// خطای درخواست صدور RFP  برای یکی از تامین کنندگان
        /// </summary>
        [Description("خطای درخواست پیشنهاد تکراری برای یکی از تامین کنندگان")]
        [Display(Name = "خطای درخواست پیشنهاد تکراری برای یکی از تامین کنندگان")]
        DuplicateRFP = 49,

        /// <summary>
        /// تاریخ پایان باید بزرگتر از تاریخ شروع باشد
        /// </summary>
        [Description("تاریخ پایان باید بزرگتر از تاریخ شروع باشد")]
        [Display(Name = "تاریخ پایان باید بزرگتر از تاریخ شروع باشد")]
        EndDateMustBeBiger = 50,

        /// <summary>
        /// bom قبلا ثبت شده است
        /// </summary>
        [Description("bom قبلا ثبت شده است")]
        [Display(Name = "bom قبلا ثبت شده است")]
        BomExist = 51,

        /// <summary>
        /// شماره قرارداد تکراری می باشد
        /// </summary>
        [Description("شماره قرارداد تکراری می باشد")]
        [Display(Name = "شماره قرارداد تکراری می باشد")]
        DuplicatContractNumber = 52,

        /// <summary>
        ///مجموع وزن شاخص ها باید برابر 100 باشد
        /// </summary>
        [Description("مجموع وزن شاخص ها باید برابر 100 باشد")]
        [Display(Name = "مجموع وزن شاخص ها باید برابر 100 باشد")]
        CriteriaWeightInvalid = 53,

        /// <summary>
        ///به دلیل استفاده شدن در ارزیابی، اماکن تغییر نوع وجود ندارد
        /// </summary>
        [Description("به دلیل استفاده شدن در ارزیابی، اماکن تغییر نوع وجود ندارد")]
        [Display(Name = "به دلیل استفاده شدن در ارزیابی، اماکن تغییر نوع وجود ندارد")]
        EvaluationTypeNotChange = 54,

        /// <summary>
        ///برنامه ریزی یافت نشد
        /// </summary>
        [Description("برنامه ریزی یافت نشد")]
        [Display(Name = "برنامه ریزی یافت نشد")]
        MrpNotFound = 55,

        /// <summary>
        ///فقط یک تیم سازمانی می توانید داشته باشید
        /// </summary>
        [Description("فقط یک تیم سازمانی می توانید داشته باشید")]
        [Display(Name = "فقط یک تیم سازمانی می توانید داشته باشید")]
        OrganizationTeamWorkError = 56,

        /// <summary>
        ///برای این قرارداد قبلا تیمی ثبت شده است
        /// </summary>
        [Description("برای این قرارداد قبلا تیمی ثبت شده است")]
        [Display(Name = "برای این قرارداد قبلا تیمی ثبت شده است")]
        TeamWorkDuplicatContract = 57,

        /// <summary>
        ///تامین کننده یافت نشد
        /// </summary>
        [Description("تامین کننده یافت نشد")]
        [Display(Name = "تامین کننده یافت نشد")]
        SupplierNotFound = 58,

        /// <summary>
        ///ایمیلی در SCM برای تامین کننده ثبت نیست
        /// </summary>
        [Description("ایمیلی در SCM برای تامین کننده ثبت نیست")]
        [Display(Name = "ایمیلی در SCM برای تامین کننده ثبت نیست")]
        SupplierDontHaveEmail = 59,

        /// <summary>
        /// وضعیت قبلا ثبت شده است
        /// </summary>
        [Description("وضعیت قبلا ثبت شده است")]
        [Display(Name = "وضعیت قبلا ثبت شده است")]
        ThisStateSetBefore = 60,

        /// <summary>
        /// خطا در پارامتر های ارسالی ایمیل
        /// </summary>
        [Description("خطا در پارامتر های ارسالی ایمیل")]
        [Display(Name = "خطا در پارامتر های ارسالی ایمیل")]
        EmailInputValidateError = 61,


        /// <summary>
        /// خطا در پارامتر های ارسالی ایمیل
        /// </summary>
        [Description("فایل یافت نشد")]
        [Display(Name = "File not found")]
        FileNotFound = 62,

        /// <summary>
        /// ارسال ایمیل با خطا مواجه شده است
        /// </summary>
        [Description("ارسال ایمیل با خطا مواجه شده است")]
        [Display(Name = "ارسال ایمیل با خطا مواجه شده است")]
        SendMailFailed = 63,

        /// <summary>
        /// آدرس ایمیل رونوشت صحیح نیست
        /// </summary>
        [Description("آدرس ایمیل رونوشت صحیح نیست")]
        [Display(Name = "آدرس ایمیل رونوشت صحیح نیست")]
        InvalidEmailCC = 64,

        /// <summary>
        /// "آدرس ایمیل گیرنده صحیح نیست
        /// </summary>
        [Description("آدرس ایمیل گیرنده صحیح نیست")]
        [Display(Name = "آدرس ایمیل گیرنده صحیح نیست")]
        InvalidEmailTo = 65,

        /// <summary>
        /// آدرس ایمیل فرستنده صحیح نیست
        /// </summary>
        [Description("آدرس ایمیل فرستنده صحیح نیست")]
        [Display(Name = "آدرس ایمیل فرستنده صحیح نیست")]
        InvalidEmailFrom = 66,

        /// <summary>
        /// موضوع ایمیل را وارد کنید
        /// </summary>
        [Description("موضوع ایمیل را وارد کنید")]
        [Display(Name = "موضوع ایمیل را وارد کنید")]
        EmailSubjectIsRequeird = 67,

        /// <summary>
        /// شرح ایمیل را وارد کنید
        /// </summary>
        [Description("شرح ایمیل را وارد کنید")]
        [Display(Name = "شرح ایمیل را وارد کنید")]
        EmailBodyIsRequeird = 68,

        /// <summary>
        /// نقش یافت نشد
        /// </summary>
        [Description("نقش یافت نشد")]
        [Display(Name = "نقش یافت نشد")]
        RoleNotFount = 69,

        /// <summary>
        /// شرح ایمیل را وارد کنید
        /// </summary>
        [Description("جهت ثبت نهایی بارگزاری حداقل یک فایل الزامی می باشد")]
        [Display(Name = "جهت ثبت نهایی بارگزاری حداقل یک فایل الزامی می باشد")]
        MinimumAttachment = 70,

        /// <summary>
        /// پارامتر ارسالی وضعیت غیر معتبر می باشد
        /// </summary>
        [Description("پارامتر ارسالی وضعیت غیر معتبر می باشد")]
        [Display(Name = "پارامتر ارسالی وضعیت غیر معتبر می باشد")]
        Invalidstatus = 80,

        /// <summary>
        /// درخواست پیشنهاد فعال برای این درخواست خرید موجود می باشد
        /// </summary>
        [Description("درخواست پیشنهاد فعال برای این درخواست خرید موجود می باشد")]
        [Display(Name = "درخواست پیشنهاد فعال برای این درخواست خرید موجود می باشد")]
        ActiveRfpExist = 81,

        /// <summary>
        /// کد پروژه تکراری می باشد
        /// </summary>
        [Description("کد پروژه تکراری می باشد")]
        [Display(Name = "Project code already exists")]
        DuplicatContractCode = 82,

        /// <summary>
        /// امکان ثبت این وضعیت وجود ندارد
        /// </summary>
        [Description("امکان ثبت این وضعیت وجود ندارد")]
        [Display(Name = "امکان ثبت این وضعیت وجود ندارد")]
        RFPSupplierStatusError = 83,

        /// <summary>
        /// امکان ویرایش نیست
        /// </summary>
        [Description("امکان ویرایش نیست.")]
        [Display(Name = "امکان ویرایش نیست.")]
        ImpossibleEdit = 84,

        /// <summary>
        /// حداقل یک آدرس انتخاب شود
        /// </summary>
        [Description("حداقل یک آدرس انتخاب شود.")]
        [Display(Name = "حداقل یک آدرس انتخاب شود.")]
        AddressValidation = 85,

        /// <summary>
        /// امکان حذف وجود ندارد
        /// </summary>
        [Description("امکان حذف وجود ندارد.")]
        [Display(Name = "عملیات با موفقیت انجام شد.")]
        RemoveIsLimited = 86,

        /// <summary>
        /// قبلا تایید شده است
        /// </summary>
        [Description("قبلا تایید شده است.")]
        [Display(Name = "قبلا تایید شده است.")]
        ApproveBefore = 87,

        /// <summary>
        /// مجموع وزن فعالیت ها نباید بیشتر از 100 شود
        /// </summary>
        [Description("مجموع وزن فعالیت ها نباید بیشتر از 100 شود.")]
        [Display(Name = "مجموع وزن فعالیت ها نباید بیشتر از 100 شود.")]
        POWeightValidation = 88,

        /// <summary>
        /// این فعالیت، کنترل کیفیت ندارد
        /// </summary>
        [Description("این فعالیت، کنترل کیفیت ندارد.")]
        [Display(Name = "این فعالیت، کنترل کیفیت ندارد.")]
        DoNotHaveQualityControl = 89,

        /// <summary>
        /// مسئولیت حمل با فروشنده می باشد
        /// </summary>
        [Description("مسئولیت حمل با فروشنده می باشد.")]
        [Display(Name = "مسئولیت حمل با فروشنده می باشد.")]
        OperationIsResponsibilitySupplier = 90,

        /// <summary>
        /// مسئولیت حمب با خریدار می باشد
        /// </summary>
        [Description("مسئولیت حمل با خریدار می باشد.")]
        [Display(Name = "مسئولیت حمل با خریدار می باشد.")]
        OperationIsResponsibilityCompany = 91,

        /// <summary>
        /// بسته ها باید در وضعیت یکسان باشند
        /// </summary>
        [Description("بسته ها باید در وضعیت یکسان باشند.")]
        [Display(Name = "بسته ها باید در وضعیت یکسان باشند.")]
        PacksStatusMustBeSameTogether = 92,

        /// <summary>
        /// وسیله نقلیه یافت نشد
        /// </summary>
        [Description("وسیله نقلیه یافت نشد.")]
        [Display(Name = "وسیله نقلیه یافت نشد.")]
        VehicleNotFound = 93,

        /// <summary>
        /// وسیله نقلیه در حال استفاده می باشد
        /// </summary>
        [Description("وسیله نقلیه در حال استفاده می باشد.")]
        [Display(Name = "وسیله نقلیه در حال استفاده می باشد.")]
        VehicleIsInUse = 94,

        /// <summary>
        /// وسیله نقلیه در حال استفاده می باشد
        /// </summary>
        [Description("وسیله نقلیه در حال استفاده می باشد.")]
        [Display(Name = "وسیله نقلیه در حال استفاده می باشد.")]
        ReceiptQuantityIsTooMuch = 95,

        /// <summary>
        /// قیمت کالا تغییر کرده است
        /// </summary>
        [Description("قیمت کالا تغییر کرده است.")]
        [Display(Name ="قیمت کالا تغییر کرده است.")]
        ProductPriceChangeInInvoice = 96,

        /// <summary>
        /// مقدار نخفیف نمی تواند بیشتر از قیمت کل باشد
        /// </summary>
        [Description("مقدار نخفیف نمی تواند بیشتر از قیمت کل باشد.")]
        [Display(Name = "مقدار نخفیف نمی تواند بیشتر از قیمت کل باشد.")]
        DiscountMostBeLosserOFTotalAmount = 97,

        /// <summary>
        /// قبلا ثبت شده است
        /// </summary>
        [Description("قبلا ثبت شده است.")]
        [Display(Name = "قبلا ثبت شده است.")]
        IsSetBefore = 98,

        /// <summary>
        /// تجهیز و متریال یافت نشد
        /// </summary>
        [Description("تجهیز و متریال یافت نشد.")]
        [Display(Name = "تجهیز و متریال یافت نشد.")]
        BomNotFound = 99,

        /// <summary>
        /// شماره ورژن تکراری می باشد
        /// </summary>
        [Description("شماره ورژن تکراری می باشد.")]
        [Display(Name = "Revision code already exists")]
        DuplicateRevisionNumber = 100,

        /// <summary>
        /// فرمت فایل اشتباه است
        /// </summary>
        [Description("فرمت فایل اشتباه است.")]
        [Display(Name = "Invalid file extension")]
        InvalidFileExtention = 101,

        /// <summary>
        /// بعد تایید اماکن حذف نیست
        /// </summary>
        [Description("بعد تایید اماکن حذف نیست.")]
        [Display(Name = "بعد تایید اماکن حذف نیست.")]
        DeleteDontAllowedAfterConfirm = 102,

        /// <summary>
        /// بعد تایید اماکن ویرایش نیست
        /// </summary>
        [Description("بعد تایید اماکن ویرایش نیست")]
        [Display(Name = "بعد تایید اماکن ویرایش نیست")]
        EditDontAllowedAfterConfirm = 103,

        /// <summary>
        /// حداقل یک تچهیز باید داشته باشد
        /// </summary>
        [Description("حداقل یک تچهیز باید داشته باشد.")]
        [Display(Name = "عملیات با موفقیت انجام شد.")]
        MinimumLimitRFPItem = 104,

        /// <summary>
        /// حداقل یک تامین کننده باید داشته باشد
        /// </summary>
        [Description("حداقل یک تامین کننده باید داشته باشد.")]
        [Display(Name = "حداقل یک تامین کننده باید داشته باشد.")]
        MinimumLimitRFPSupplier = 105,

        /// <summary>
        /// مدرک خرید حتما باید کالا داشته باشد
        /// </summary>
        [Description("مدرک خرید حتما باید کالا داشته باشد.")]
        [Display(Name = "مدرک خرید حتما باید کالا داشته باشد.")]
        DocumentPurchaseValidation = 106,

        /// <summary>
        /// شرح تغییر الزامی می باشد.
        /// </summary>
        [Description("شرح تغییر الزامی می باشد.")]
        [Display(Name = "Please enter reason of registration")]
        RequiredRevisionDescription = 107,

        /// <summary>
        /// لطفا اطلاعات شناسه ملی و کد اقتصادی تامین کننده را تکمیل کنید.
        /// </summary>
        [Description("لطفا اطلاعات شناسه ملی و کد اقتصادی تامین کننده را تکمیل کنید.")]
        [Display(Name = "لطفا اطلاعات شناسه ملی و کد اقتصادی تامین کننده را تکمیل کنید.")]
        SupplierInformationItsNotCompelet = 108,

        /// <summary>
        /// امکان ثبت کالاهای یکسان نمی باشد
        /// </summary>
        [Description("امکان ثبت کالاهای یکسان نمی باشد.")]
        [Display(Name = "امکان ثبت کالاهای یکسان نمی باشد.")]
        ImpossibleDuplicateProduct = 109,

        /// <summary>
        /// بدلیل ثبت اطلاعات کنترل کیفیت، امکان حذف فعالیت وجود ندارد.
        /// </summary>
        [Description("بدلیل ثبت اطلاعات کنترل کیفیت، امکان حذف فعالیت وجود ندارد.")]
        [Display(Name = "بدلیل ثبت اطلاعات کنترل کیفیت، امکان حذف فعالیت وجود ندارد.")]
        DontAllowDeleteAfterQC = 110,

        /// <summary>
        /// میزان پارت وارد شده باید ضریبی از میزان موضوع باشد
        /// </summary>
        [Description("میزان پارت وارد شده باید ضریبی از میزان موضوع باشد.")]
        [Display(Name = "میزان پارت وارد شده باید ضریبی از میزان موضوع باشد.")]
        PartListCoefficientUseValidation = 111,

        /// <summary>
        /// امکان ثبت کنترل کیفی نیست
        /// </summary>
        [Description("امکان ثبت کنترل کیفی نیست")]
        [Display(Name = "امکان ثبت کنترل کیفی نیست")]
        ImpossibleAddQC = 112,

        /// <summary>
        /// امکان عدم پذیرش پارت وجود ندارد
        /// </summary>
        [Description("امکان عدم پذیرش پارت وجود ندارد")]
        [Display(Name = "امکان عدم پذیرش پارت وجود ندارد")]
        ImpossibleRejectPartListReceipt = 113,

        /// <summary>
        /// اطلاعات ورودی تکراری می باشد.
        /// </summary>
        [Description("اطلاعات ورودی تکراری می باشد.")]
        [Display(Name = "اطلاعات ورودی تکراری می باشد.")]
        DuplicateInformation = 114,

        /// <summary>
        /// امکان ثبت ویرایش جدید نمی باشد.
        /// </summary>
        [Description("امکان ثبت ویرایش جدید نمی باشد.")]
        [Display(Name = "Registering a new version is not allowed")]
        ImposibleAddRevision = 115,

        /// <summary>
        /// خطا در پارامتر های ارسالی ایمیل
        /// </summary>
        [Description("حجم فایل بارگزاری شده بیش از حد مجاز می باشد.")]
        [Display(Name = "File size is  larger than max allowed")]
        FileSizeError = 116,

        /// <summary>
        /// به دلیل عدم تایید بازرسی امکان شروع عملیات نمی باشد
        /// </summary>
        [Description("به دلیل عدم تایید بازرسی امکان شروع عملیات نمی باشد")]
        [Display(Name = "به دلیل عدم تایید بازرسی امکان شروع عملیات نمی باشد")]
        NoInspection = 117,
        
        /// <summary>
        /// کدینگ فرم یافت نشد
        /// </summary>
        [Description("کدینگ فرم یافت نشد.")]
        [Display(Name = "Form coding not found")]
        NotFoundFormConfig = 118,
        
        /// <summary>
        /// کد تکراری وارد شده است
        /// </summary>
        [Description("کد تکراری وارد شده است.")]
        [Display(Name = "Fixed part already exists")]
        DuplicateFormFixPart = 119,
        
        /// <summary>
        /// به دلیل ثبت فم امکان ویرایش نمی باشد
        /// </summary>
        [Description("به دلیل ثبت فرم امکان ویرایش نمی باشد.")]
        [Display(Name = "Editing information after registration is not allowed")]
        ImpossibleEditFormConfig = 120,

        /// <summary>
        /// قالب pdf پیدا نشد.
        /// </summary>
        [Description("قالب pdf پیدا نشد.")]
        [Display(Name = "Pdf template not found")]
        PdfTemplateNotFound =121,

        /// <summary>
        /// ناحیه ای با این نام وجود دارد.
        /// </summary>
        [Description("ناحیه ای با این نام وجود دارد.")]
        [Display(Name = "Area already exists")]
        AreaExistAlready = 122,

        /// <summary>
        /// ناحیه وارد شده در این پروژه موجود نیست.
        /// </summary>
        [Description("ناحیه وارد شده در این پروژه موجود نیست.")]
        [Display(Name = "ناحیه وارد شده در این پروژه موجود نیست.")]
        AreaNotExistInContract = 123,

        /// <summary>
        /// ناحیه وارد شده وجود ندارد.
        /// </summary>
        [Description("ناحیه وارد شده وجود ندارد.")]
        [Display(Name = "Area dose not exist")]
        AreaNotExist = 124,

        /// <summary>
        /// کاربر مشتری وجود ندارد
        /// </summary>
        [Description("کاربر مشتری وجود ندارد")]
        [Display(Name = "Customer 's user not found")]
        CompanyUserNotFound = 125,

        /// <summary>
        /// مشاور وجود ندارد
        /// </summary>
        [Description("مشاور یافت نشد")]
        [Display(Name = "Consultant not found")]
        ConsultantNotFound = 126,

        /// <summary>
        /// نام کاربری یا کلمه عبور صحیح نمیباشد
        /// </summary>
        [Description("نام کاربری یا کلمه عبور صحیح نمیباشد")]
        [Display(Name = "Username or Password invalid")]
        UserNameOrPasswordNotCorrect = 127,

        /// <summary>
        ///به دلیل وجود وابستگی به این موجودیت، امکان تغییر آن وجود ندارد، ابتدا نسبت به حذف وابسته های آن اقدام کنید
        /// </summary>
        [Description("به دلیل وجود وابستگی به این موجودیت، امکان تغییر آن وجود ندارد، ابتدا نسبت به حذف وابسته های آن اقدام کنید")]
        [Display(Name = "Due to dependence on this data,  this is impossible to change it, first remove its dependents")]
        EditDontAllowedBeforeSubset = 128,

        /// <summary>
        /// کد فعالیت اجرایی تکراری است
        /// </summary>
        [Description("کد فعالیت اجرایی تکراری است")]
        [Display(Name = "کد فعالیت اجرایی تکراری است")]
        DuplicatOperationCode = 129,

        /// <summary>
        /// فعالیت اجرایی غیر فعال است
        /// </summary>
        [Description("فعالیت اجرایی غیر فعال است")]
        [Display(Name = "فعالیت اجرایی غیر فعال است")]
        OperationIsDeactive = 130,

        /// <summary>
        /// گروه فعالیت اجرایی مورد نظر یافت نشد! 
        /// </summary>
        [Description("گروه فعالیت اجرایی مورد نظر یافت نشد")]
        [Display(Name = "گروه فعالیت اجرایی مورد نظر یافت نشد")]
        OperationGroupNotExist = 131,

        /// <summary>
        /// فعالیت اجرایی مورد نظر قبلا شروع شده است! 
        /// </summary>
        [Description("فعالیت اجرایی مورد نظر قبلا شروع شده است")]
        [Display(Name = "فعالیت اجرایی مورد نظر قبلا شروع شده است")]
        OperationStartedAlready = 132,

        /// <summary>
        /// فعالیت اجرایی مورد نظر قبلا شروع شده است! 
        /// </summary>
        [Description("جمع درصد پیشرفت نمیتواند بیش از 100 باشد")]
        [Display(Name = "جمع درصد پیشرفت نمیتواند بیش از 100 باشد")]
        ProgressPercentSumCannotBeOver100 = 133,

        /// <summary>
        /// عملیات مورد نظر فعالیت انجام نشده دارد! 
        /// </summary>
        [Description("عملیات مورد نظر فعالیت انجام نشده دارد")]
        [Display(Name = "عملیات مورد نظر فعالیت انجام نشده دارد")]
        OperationHasNotCompletedActivity = 134,

        /// <summary>
        /// عملیات مورد نظر قابل نهایی سازی نیست! 
        /// </summary>
        [Description("عملیات مورد نظر قابل نهایی سازی نیست")]
        [Display(Name = "عملیات مورد نظر قابل نهایی سازی نیست")]
        OperationCannotBeConfirm = 135,

        /// <summary>
        /// پیشرفت عملیات مورد نظر کامل نشده است! 
        /// </summary>
        [Description("پیشرفت عملیات مورد نظر کامل نشده است")]
        [Display(Name = "پیشرفت عملیات مورد نظر کامل نشده است")]
        OperationProgressNotCompleted = 136,

        /// <summary>
        /// عملیات مورد نظر کامل شده است! 
        /// </summary>
        [Description("عملیات مورد نظر کامل شده است")]
        [Display(Name = "عملیات مورد نظر کامل شده است")]
        OperationActivityHasDone = 137,

        /// <summary>
        /// عملیات نهایی سازی نشده است! 
        /// </summary>
        [Description("عملیات نهایی سازی نشده است")]
        [Display(Name = "عملیات نهایی سازی نشده است")]
        OperationNotConfirm = 138,

        /// <summary>
        /// عملیات نهایی سازی شده است! 
        /// </summary>
        [Description("عملیات نهایی سازی شده است")]
        [Display(Name = "عملیات نهایی سازی شده است")]
        OperationConfirmAlready = 139,

        /// <summary>
        /// ویرایش نهایی نشده است! 
        /// </summary>
        [Description("ویرایش نهایی نشده است")]
        [Display(Name = "ویرایش نهایی نشده است")]
        RevisionNotConfirmed = 140,



        /// <summary>
        /// فولدری با این نام از قبل وجود دارد! 
        /// </summary>
        [Description("فولدری با این نام از قبل وجود دارد")]
        [Display(Name = "Folder name  already exists")]
        DuplicateDirectory = 142,

        /// <summary>
        /// فولدر مقصد وجود ندارد! 
        /// </summary>
        [Description("فولدر مقصد وجود ندارد")]
        [Display(Name = "Destination folder dose not exist")]
        DestinationDirectoryNotExist = 143,

        /// <summary>
        /// در فولدر مقصد فولدری با این نام وجود دارد! 
        /// </summary>
        [Description("در فولدر مقصد فولدری با این نام وجود دارد")]
        [Display(Name = "There is a file with same name in the destination")]
        DuplicateDirectoryInDestination = 144,

        /// <summary>
        /// فولدر مبدا و مقصد یکسان است! 
        /// </summary>
        [Description("فولدر مبدا و مقصد یکسان است")]
        [Display(Name = "Origin and destination folder can not be same.")]
        SourceAndDestinationIsSame = 145,

        /// <summary>
        /// فایلی با نام مشابه در این فولدر وجود دارد! 
        /// </summary>
        [Description("فایلی با نام مشابه در این فولدر وجود دارد")]
        [Display(Name = "There is a file with same name in the destination")]
        DuplicateFile = 146,

        /// <summary>
        /// حرکت فایل با مشکل مواجه شد! 
        /// </summary>
        [Description("انتقال فایل با مشکل مواجه شد")]
        [Display(Name = "Moving file failed")]
        MoveFileFailed = 147,

        /// <summary>
        /// کپی فایل با مشکل مواجه شد! 
        /// </summary>
        [Description("کپی فایل با مشکل مواجه شد")]
        [Display(Name = "Copying file failed")]
        CopyFileFailed = 148,


        /// <summary>
        /// انجام عملیات با خطا مواجه شد! 
        /// </summary>
        [Description("انجام عملیات با خطا مواجه شد")]
        [Display(Name = "Operation failed")]
        OperationFailed = 149,

        /// <summary>
        /// شرح پروژه نباید بیش از 800 کاراکتر باشد! 
        /// </summary>
        [Description("شرح پروژه نباید بیش از 800 کاراکتر باشد")]
        [Display(Name = "Project description max length is 800 character")]
        DescriptionInputLengthInvalid = 150,

        /// <summary>
        /// کد پروژه نمی تواند شامل کاراکترهای فارسی باشد! 
        /// </summary>
        [Description("کد پروژه نمی تواند شامل کاراکترهای فارسی باشد")]
        [Display(Name = "Project code should not contain Persian characters")]
        ContractCodeHasPersianCharachter = 151,

        /// <summary>
        /// سرویس ها نمی تواند خالی باشد! 
        /// </summary>
        [Description("سرویس ها نمی تواند خالی باشد")]
        [Display(Name = "At least one service must be enabled")]
        ServicesCannotBeEmpty = 152,

        /// <summary>
        /// سرویس مدیریت مدارک برای شما قابل دسترس نیست! 
        /// </summary>
        [Description("سرویس مدیریت مدارک برای شما قابل دسترس نیست")]
        [Display(Name = "You don't have permission to access document management")]
        DocumentServicesNotAvailable = 153,

        /// <summary>
        /// سرویس فایل درایو برای شما قابل دسترس نیست! 
        /// </summary>
        [Description("سرویس فایل درایو برای شما قابل دسترس نیست")]
        [Display(Name = "You don't have permission to access filedrive")]
        FileDriveServicesNotAvailable = 154,

        /// <summary>
        /// سرویس مدیریت زنجیره تامین برای شما قابل دسترس نیست! 
        /// </summary>
        [Description("سرویس مدیریت زنجیره تامین برای شما قابل دسترس نیست")]
        [Display(Name = "You don't have permission to access Procurement Management")]
        PurchaseServicesNotAvailable = 155,

        /// <summary>
        /// سرویس مدیریت اجرا برای شما قابل دسترس نیست! 
        /// </summary>
        [Description("سرویس مدیریت اجرا برای شما قابل دسترس نیست")]
        [Display(Name = "You don't have permission to access Opration Management")]
        ConstructionServicesNotAvailable = 156,

        /// <summary>
        /// سرویس ها نمی تواند خالی باشد! 
        /// </summary>
        [Description("اطلاعات سرویس های قابل استفاده در دسترس نیست")]
        [Display(Name = "Service plan not found")]
        ServiceInformationNotAvailable = 157,

        /// <summary>
        /// در صورت ثبت ترانسمیتال برای مشتری امکان ویرایش وجود ندارد! 
        /// </summary>
        [Description("در صورت ثبت ترانسمیتال امکان ویرایش وجود ندارد")]
        [Display(Name = "Editing information is not allowed after transmittal registration")]
        EditNotAllowAfterCreateTransmittal = 158,


        /// <summary>
        /// در صورت ثبت ترانسمیتال برای مشاور امکان ویرایش وجود ندارد! 
        /// </summary>
        [Description("در صورت ثبت ترانسمیتال امکان ویرایش وجود ندارد")]
        [Display(Name = "Editing information is not allowed after transmittal registration")]
        EditNotAllowAfterCreateTransmittalForConsultant = 159,

        /// <summary>
        /// پلن سرویس فعال نمی باشد! 
        /// </summary>
        [Description("پلن سرویس فعال نمی باشد")]
        [Display(Name = "Service plan is not active")]
        PlanServiceNotStarted = 160,

        /// <summary>
        /// پلن سرویس منقضی شده است! 
        /// </summary>
        [Description("پلن سرویس منقضی شده است")]
        [Display(Name = " The service plan has expired")]
        PlanServiceExpired = 161,

        /// <summary>
        /// امکان اشتراک گذاری وجود ندارد! 
        /// </summary>
        [Description("امکان اشتراک گذاری وجود ندارد")]
        [Display(Name = "Only owner can share")]
        OnlyOwnerCanShare = 162,

        /// <summary>
        /// استفاده از کارکترهای '\\' و '/' در عنوان فایل و فولدر غیرمجاز است! 
        /// </summary>
        [Description("استفاده از کاراکترهای \n '\\','/','\"','*','?','<','>','|',':' \n غیر مجاز است")]
        [Display(Name = "Folder name has invalid character ('\\','/','\"','*','?','<','>','|',':')")]
        InvalidCharacter = 163,

        /// <summary>
        /// تجهیز قبلا به عنوان مجموعه ثبت شده است! 
        /// </summary>
        [Description("تجهیز قبلا به عنوان مجموعه ثبت شده است")]
        [Display(Name = "تجهیز قبلا به عنوان مجموعه ثبت شده است")]
        HasComponentTypeBom = 164,

        /// <summary>
        /// تجهیز قبلا به لیست اضافه شده است! 
        /// </summary>
        [Description("تجهیز قبلا به لیست اضافه شده است")]
        [Display(Name = "تجهیز قبلا به لیست اضافه شده است")]
        HasPartTypeBom = 165,

        /// <summary>
        /// امکان کپی کردن تجهیز وجود ندارد! 
        /// </summary>
        [Description("امکان کپی کردن تجهیز وجود ندارد")]
        [Display(Name = "عملیات با موفقیت انجام شد.")]
        HasBothTypeBom = 166,

        /// <summary>
        /// تغییر گروه کالا  به علت وجود وابستگی امکانپذیر نیست! 
        /// </summary>
        [Description("تغییر گروه کالا  به علت وجود وابستگی امکانپذیر نیست ")]
        [Display(Name = "تغییر گروه کالا  به علت وجود وابستگی امکانپذیر نیست ")]
        ProductGroupCantBeEdit = 167,

        /// <summary>
        /// برای افزایش تعداد رکورد مورد نظر را ویرایش کنید! 
        /// </summary>
        [Description("برای افزایش، تعداد رکورد مورد نظر را ویرایش کنید")]
        [Display(Name = "برای افزایش، تعداد رکورد مورد نظر را ویرایش کنید")]
        BomQuantityCanAdjust = 168,

        /// <summary>
        /// تعداد نمیتواند از مقدار برنامه ریزی شده کمتر باشد! 
        /// </summary>
        [Description("تعداد نمیتواند از مقدار برنامه ریزی شده کمتر باشد")]
        [Display(Name = "تعداد نمیتواند از مقدار برنامه ریزی شده کمتر باشد")]
        QuantityGreaterThanRemaind = 170,

        /// <summary>
        /// امکان نهایی سازی بدون گردش کار وجود ندارد! 
        /// </summary>
        [Description("امکان نهایی سازی بدون گردش کار وجود ندارد")]
        [Display(Name = "امکان نهایی سازی بدون گردش کار وجود ندارد")]
        CantConfirmPurchaseRequestWithoutUserConfirm = 171,

        /// <summary>
        /// تعداد نمیتواند از مقدار برنامه ریزی شده کمتر باشد! 
        /// </summary>
        [Description("تعداد نمیتواند از مقدار برنامه ریزی شده بیشتر باشد")]
        [Display(Name = "تعداد نمیتواند از مقدار برنامه ریزی شده بیشتر باشد")]
        QuantityCantGreaterThanRemaind = 172,

        /// <summary>
        /// امکان لغو در صورت وجود آیتم ها در قرارداد خرید وجود ندارد! 
        /// </summary>
        [Description("امکان لغو در صورت وجود آیتم ها در قرارداد خرید وجود ندارد")]
        [Display(Name = "امکان لغو در صورت وجود آیتم ها در قرارداد خرید وجود ندارد")]
        RFPItemsUsedInPRContract = 173,

        /// <summary>
        /// امکان ویرایش بعد از انتخاب برنده وجود ندارد! 
        /// </summary>
        [Description("امکان ویرایش بعد از انتخاب برنده وجود ندارد")]
        [Display(Name = "امکان ویرایش بعد از انتخاب برنده وجود ندارد")]
        RFPItemsCantBeEditAfterVendorSelection = 174,


        /// <summary>
        /// امکان حذف پیش فاکتور پس از انتخاب برنده وجود ندارد! 
        /// </summary>
        [Description("امکان حذف پیش فاکتور پس از انتخاب برنده وجود ندارد")]
        [Display(Name = "امکان حذف پیش فاکتور پس از انتخاب برنده وجود ندارد")]
        ProFormaCantDeleteAfterVendorSelection = 175,

        /// <summary>
        /// تعداد نمیتواند از مقدار برنامه ریزی شده کمتر باشد! 
        /// </summary>
        [Description("مقدار درخواستی از تعداد RFP شده کمتر است")]
        [Display(Name = "مقدار درخواستی از تعداد RFP شده کمتر است")]
        QuantityCantLessThanRFPDone = 176,


        /// <summary>
        /// در صورت ثبت سفارش امکان لغو قرارداد وجود ندارد! 
        /// </summary>
        [Description("در صورت ثبت سفارش امکان لغو قرارداد وجود ندارد")]
        [Display(Name = "در صورت ثبت سفارش امکان لغو قرارداد وجود ندارد")]
        PrContractCantCancelIfHasPo = 177,

        /// <summary>
        /// در صورت ثبت پاسخ نمیتوان امکان حذف استعلام وجود ندارد! 
        /// </summary>
        [Description("در صورت ثبت پاسخ نمیتوان امکان حذف استعلام وجود ندارد")]
        [Display(Name = "در صورت ثبت پاسخ نمیتوان امکان حذف استعلام وجود ندارد")]
        InqueryCantBeRemove = 178,

        /// <summary>
        /// امکان اضافه کردن استعلام بعد از ارزیابی وجود ندارد! 
        /// </summary>
        [Description("امکان اضافه کردن استعلام بعد از ارزیابی وجود ندارد")]
        [Display(Name = "امکان اضافه کردن استعلام بعد از ارزیابی وجود ندارد")]
        InqueryCantBeAdd = 179,

        /// <summary>
        /// تعداد نمیتواند از مقدار درخواست خرید کمتر باشد! 
        /// </summary>
        [Description("تعداد نمیتواند از مقدار درخواست خرید کمتر باشد")]
        [Display(Name = "تعداد نمیتواند از مقدار درخواست خرید کمتر باشد")]
        QuantityGreaterThanDoneStock = 180,

        /// <summary>
        /// امکان ارزیابی قبل از ثبت پروپوزال وجود ندارد! 
        /// </summary>
        [Description("امکان ارزیابی قبل از ثبت پروپوزال وجود ندارد")]
        [Display(Name = "امکان ارزیابی قبل از ثبت پروپوزال وجود ندارد")]
        EvaluateProposalNotpossibleBeforRegPoropsal = 181,

        /// <summary>
        /// مجموع درصد پرداخت ها باید برابر صد باشد! 
        /// </summary>
        [Description("مجموع درصد پرداخت ها باید برابر صد باشد")]
        [Display(Name = "مجموع درصد پرداخت ها باید برابر صد باشد")]
        PaymentPercentShouldBeEqualHundred = 182,

        /// <summary>
        /// تعداد نمیتواند صفر باشد! 
        /// </summary>
        [Description("تعداد نمیتواند صفر باشد")]
        [Display(Name = "تعداد نمیتواند صفر باشد")]
        QuantityCantLessOrEqualZero = 183,

        /// <summary>
        /// بعد از تایید بازرسی پکینگ امکان لغو سفارش وجود ندارد! 
        /// </summary>
        [Description("بعد از تایید بازرسی پکینگ امکان لغو سفارش وجود ندارد")]
        [Display(Name = "بعد از تایید بازرسی پکینگ امکان لغو سفارش وجود ندارد")]
        PoCantCancelIfHasConfirmedQCPack = 184,

        /// <summary>
        /// مقدار وارد شده بیش از سقف مجاز درخواست پرداخت است! 
        /// </summary>
        [Description("مقدار وارد شده بیش از سقف مجاز درخواست پرداخت است")]
        [Display(Name = "مقدار وارد شده بیش از سقف مجاز درخواست پرداخت است")]
        PoPaymentAmountUpperThenValidAmount = 185,

        /// <summary>
        /// امکان ثبت درخواست پرداخت وجود ندارد! 
        /// </summary>
        [Description("امکان ثبت درخواست پرداخت وجود ندارد")]
        [Display(Name = "امکان ثبت درخواست پرداخت وجود ندارد")]
        PoPaymentCantBeCreate = 186,

        /// <summary>
        /// امکان تغییر نتیجه بازرسی وجود ندارد! 
        /// </summary>
        [Description("امکان تغییر نتیجه بازرسی وجود ندارد")]
        [Display(Name = "امکان تغییر نتیجه بازرسی وجود ندارد")]
        InspecitonResultCantBeModified = 187,

        /// <summary>
        /// امکان ثبت بدون بارگزاری فایل وجود ندارد! 
        /// </summary>
        [Description("امکان ثبت بدون بارگزاری فایل وجود ندارد")]
        [Display(Name = "امکان ثبت بدون بارگزاری فایل وجود ندارد")]
        CannotSaveWithoutAttachment = 188,

        /// <summary>
        /// امکان حذف درخواست پرداخت وجود ندارد! 
        /// </summary>
        [Description("امکان حذف درخواست پرداخت وجود ندارد")]
        [Display(Name = "امکان حذف درخواست پرداخت وجود ندارد")]
        PendingForPaymentHasPayment = 189,


        /// <summary>
        /// امکان انجام عملیات روی سفارش لغو شده وجود ندارد! 
        /// </summary>
        [Description("امکان انجام عملیات روی سفارش لغو شده وجود ندارد")]
        [Display(Name = "امکان انجام عملیات روی سفارش لغو شده وجود ندارد")]
        CantDoneBecausePOCanceled = 190,

        /// <summary>
        /// مقدار در خواستی نمیتواند از موجودی انبار بیشتر باشد! 
        /// </summary>
        [Description("مقدار در خواستی نمیتواند از موجودی انبار بیشتر باشد")]
        [Display(Name = "مقدار در خواستی نمیتواند از موجودی انبار بیشتر باشد")]
        QuantityCantBeGreaterThenInventory = 191,

        /// <summary>
        /// مقدار درخواستی نمیتواند برای تمامی آیتم ها صفر باشد! 
        /// </summary>
        [Description("مقدار درخواستی نمیتواند برای تمامی آیتم ها صفر باشد ")]
        [Display(Name = "مقدار درخواستی نمیتواند برای تمامی آیتم ها صفر باشد ")]
        AllQuantityCantBeZeroInWarehouseDispatch = 192,

        /// <summary>
        /// به علت اتمام موجودی انبار امکان ثبت وجود ندارد! 
        /// </summary>
        [Description("به علت اتمام موجودی انبار امکان ثبت وجود ندارد")]
        [Display(Name = "به علت اتمام موجودی انبار امکان ثبت وجود ندارد")]
        InventoryIsZero = 193,

        /// <summary>
        /// تاریخ تحویل نمیتواند قبل از تاریخ شروع قرارداد باشد! 
        /// </summary>
        [Description("تاریخ تحویل نمیتواند قبل از تاریخ شروع قرارداد باشد.")]
        [Display(Name = "تاریخ تحویل نمیتواند قبل از تاریخ شروع قرارداد باشد.")]
        DeliverDateCantBelessThenContractStartDate = 194,

        /// <summary>
        /// درصد پیشرفت نمیتواند منفی باشد! 
        /// </summary>
        [Description("درصد پیشرفت نمیتواند منفی باشد")]
        [Display(Name = "درصد پیشرفت نمیتواند منفی باشد")]
        ProgressPercentCantBelowZero = 195,


        /// <summary>
        /// برای تمام پرسش ها پاسخ ثبت نشده است 
        /// </summary>
        [Description("برای تمام پرسش ها پاسخ ثبت نشده است")]
        [Display(Name = "All items should be replyed")]
        ReplyCountCantLessThenQuestion = 196,

        /// <summary>
        /// گروه مدرکی با این عنوان از قبل ثبت شده است! 
        /// </summary>
        [Description("گروه مدرکی با این عنوان از قبل ثبت شده است")]
        [Display(Name = "Document group already exists")]
        DocumentGroupIsExistAllready = 196,




        /// <summary>
        /// پاسخ نمی تواند خالی باشد.
        /// </summary>
        [Description("پاسخ نمی تواند خالی باشد")]
        [Display(Name = "Please enter your reply")]
        ReplyDescriptionCantBeNullOrEmpty = 199,

        /// <summary>
        /// پرسش نمی تواند خالی باشد.
        /// </summary>
        [Description("پرسش نمی تواند خالی باشد")]
        [Display(Name = "Please enter your description")]
        QuestionDescriptionCantBeNullOrEmpty = 200,

        /// <summary>
        /// مسئول انتخاب شده دسترسی لازم را ندارد.
        /// </summary>
        [Description("مسئول انتخاب شده دسترسی لازم را ندارد")]
        [Display(Name = "Responsible does not have access")]
        ResponsibleHaveNotAcess = 201,

        /// <summary>
        /// فقط مسئول فعالیت قابلیت تغییر در فعالیت را دارد.
        /// </summary>
        [Description("فقط مسئول فعالیت قابلیت تغییر در فعالیت را دارد")]
        [Display(Name = "Only acivity responsible have permission to change status")]
        OnlyResponsibleCanMakeChangeInActivity = 202,

        /// <summary>
        /// ویرایش قبلا تایید شده است.
        /// </summary>
        [Description("ویرایش قبلا تایید شده است")]
        [Display(Name = "Revision already confirmed")]
        RevisionAlreadyConfirm = 203,

        /// <summary>
        /// ویرایش قبلا تایید شده است.
        /// </summary>
        [Description("گردش کار کاربر تایید کننده ای ندارد")]
        [Display(Name = "Confirmation Wrokflow does not contain any user")]
        ConfirmationWorkflowHaveNotUser = 204,

        /// <summary>
        /// ویرایش قبلا تایید شده است.
        /// </summary>
        [Description("ناحیه نمیتواند بیش از 20 کاراکتر باشد")]
        [Display(Name = "Area title length invalid")]
        AreaTitleLegnthOverLimited = 205,

        /// <summary>
        /// کد پروژه نمی تواند شامل کاراکترهای فارسی باشد! 
        /// </summary>
        [Description("کد پروژه شامل کاراکترهای نامعتبر است")]
        [Display(Name = "Project code has invalid character")]
        ContractCodeCharacterInvalid = 206,

        /// <summary>
        /// حجم فایل پیوست ایمیل بیش از حد مجاز است! 
        /// </summary>
        [Description("حجم فایل پیوست ایمیل بیش از حد مجاز است")]
        [Display(Name = "Email attachment length not valid")]
        EmailAttachmentLengthNotValid = 207,

        /// <summary>
        /// لینک بازیابی کلمه عبور منقضی شده است! 
        /// </summary>
        [Description("لینک بازیابی کلمه عبور منقضی شده است")]
        [Display(Name = "Password recovery link expired")]
        RecoveryLinkExpired = 208,

        /// <summary>
        /// بازیابی کلمه عبور با خطا مواجه شد! 
        /// </summary>
        [Description("بازیابی کلمه عبور با خطا مواجه شد")]
        [Display(Name = "Password recovery failed")]
        RecoveryPasswordFailed = 209,
    }
}
