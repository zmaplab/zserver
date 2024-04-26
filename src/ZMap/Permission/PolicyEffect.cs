namespace ZMap.Permission;

public record PolicyEffect
{
    public string Name { get; }

    private PolicyEffect(string name)
    {
        Name = name;
    }

    /// <summary>
    /// 存在任意一条通过
    /// </summary>
    public static readonly PolicyEffect Allow = new(nameof(Allow));

    /// <summary>
    /// 全部通过
    /// </summary>
    public static readonly PolicyEffect Deny = new(nameof(Deny));

    /// <summary>
    /// 存在任意一条通过 && 全部通过
    /// </summary>
    public static readonly PolicyEffect AllowAndDeny = new(nameof(AllowAndDeny));
}