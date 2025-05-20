using System;
using System.Text;
using Newtonsoft.Json;

namespace Common.com.serialization
{
    public abstract class JsonSerializable
    {
        
        // For Objects to be Serializable, they have to have "[Serializable]" as Attribute
        public byte[] Serialize()
        // Converts Object into Byte Stream
        {
            
            try
            {
                return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this,
                    new JsonSerializerSettings()
                    { 
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    }));
            }
            catch (Exception e)
            {
                Console.Error.WriteAsync(e.Message+"\n"+e.StackTrace);
                
            }

            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
        }

        public static JsonSerializable Deserialize(byte[] byteArray)
        // Converts Byte Array into Object
        {
            try
            { 
                string str = Encoding.UTF8.GetString(byteArray);
                return JsonConvert.DeserializeObject<JsonSerializable>(str,new GameObjectConverter());
            }
            catch (Exception e)
            {
                Console.Error.WriteAsync(e.Message+"\n"+e.StackTrace);
                // Code wasnt able to run properly
                // handle Exeption here
                return default;
            }
        }
        public abstract int ClassNumber { get;  }
    }
}