using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Helpers;
internal class TypeHelpers
{
    public PropertyDescriptorCollection GetPropertiesOfAnObject(object obj)
    {
        var props = System.ComponentModel.TypeDescriptor.GetProperties(obj);
        return props;
    }
}
