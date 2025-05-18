using System.Text.Json.Serialization;
using ServiceUploader.Models;

namespace ServiceUploader;

[JsonSourceGenerationOptions(WriteIndented = false, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(UploadImageInfoModel))]

[JsonSerializable(typeof(DeviceProjectInfoResponse))]
[JsonSerializable(typeof(IEnumerable<DeviceProjectEntryInfo>))]
[JsonSerializable(typeof(DeviceProjectEntryInfo))]
[JsonSerializable(typeof(IEnumerable<DeviceImageInfoResponse>))]
[JsonSerializable(typeof(DeviceImageInfoResponse))]
internal partial class RequestSerializerContext : JsonSerializerContext
{
    
}