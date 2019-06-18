using DynamicForms.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.IO;
using System.Xml.Serialization;
using Xunit;

namespace DynamicForms.UnitTests
{
    public class SerializationUnitTests
    {
        [Fact]
        public void Test1()
        {
            dynamic a = new JObject();
            a.Test2 = "abc";

            var value= JsonConvert.SerializeObject(a);
        }


        public class ViewModel : DynamicForm
        {

            public string Test { get; set; } = "value";
        }
    }
}
