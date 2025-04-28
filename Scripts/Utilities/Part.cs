[System.Serializable]
public class Part
{
    public string Name;
    public bool Enable;

    public Part(string name, bool enable)
    {
        Name = name;
        Enable = enable;
    }
}