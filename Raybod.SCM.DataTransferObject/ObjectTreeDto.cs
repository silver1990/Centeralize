using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Raybod.SCM.DataTransferObject
{
    public class ObjectTreeDto
    {
        public Guid Value { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Guid ParentId { get; set; }
        public string Label { get; set; }
    }


    public class FinalObjectTreeModelDto
    {
        [JsonProperty("value")]
        public Guid Value { get; set; }
        [JsonProperty("label")]
        public string Label { get; set; }
        [JsonProperty("showCheckbox")]
        public bool ShowCheckbox { get; set; } = false;
        //public string Icon { get; set; } = "<i className='fa fa-folder fa-lg text-warning'></i>";
        //public string ClassName { get; set; } = "TreeClass";
        [JsonProperty("disabled")]
        public bool Disabled { get; set; } =false;
       
        public bool ShouldSerializeChildren()
        {
            return Children.Count > 0;
        }
        [JsonProperty("children", NullValueHandling = NullValueHandling.Ignore)]
        public List<FinalObjectTreeModelDto> Children { get; set; }
   
    }
}
