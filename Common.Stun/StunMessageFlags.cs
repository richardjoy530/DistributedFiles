namespace Common.Stun
{
    [Flags]
    public enum StunMessageFlags: byte
    {
        None,
        ChangeIp,
        ChangePort,
        ChangeBoth,
        ResponseAddress,
    }
}
