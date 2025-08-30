using System.Text.Json.Serialization;
using versioning_manager_api.Models.Requests.Images;
using versioning_manager_api.Models.Responses.Images;

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