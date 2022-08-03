namespace Raybod.SCM.DataTransferObject.Role.Permission
{
    public class PermissionDto
    {
        public int Id { get; set; }

        public bool IsCritical { get; set; } = false;

        /// <summary>
        /// نام پرمیشن به انگلیسی - برای استفاده در سیستم
        /// </summary> 
        public string Name { get; set; }

        /// <summary>
        /// نام پرمیشن - برای نمایش به کاربر
        /// </summary> 
        public string DisplayName { get; set; }

        /// <summary>
        /// توضیحات مربوط به پرمیشن
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// نام پرشین گروه بنده
        /// </summary>
        public string Category { get; set; }
    }
}
