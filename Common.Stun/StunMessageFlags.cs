namespace Common.Stun
{
    [Flags]
    public enum StunMessageFlags: byte
    {
        None,
        ChangeAddress,
        ChangePort,
        ChangeBoth,
        ResponseAddress,
    }
}
