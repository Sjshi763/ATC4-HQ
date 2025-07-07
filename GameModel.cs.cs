using System;
using System.Text.Json.Serialization; // 用于 JSON 序列化

namespace ATC4_HQ.Models // 确保这个命名空间是正确的
{
    public class GameModel
    {
        // Id 通常由服务端或数据库管理，客户端在添加时可以不设置
        // [JsonPropertyName("Id")]
        // public int Id { get; set; } 

        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty; // 游戏名称

        [JsonPropertyName("Path")]
        public string Path { get; set; } = string.Empty; // 游戏安装路径
    }
}