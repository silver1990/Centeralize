using System.Collections.Generic;

namespace Raybod.SCM.ModuleApi.Helper
{
    public class ModelStateError
    {
        public string Key { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();
    }
}