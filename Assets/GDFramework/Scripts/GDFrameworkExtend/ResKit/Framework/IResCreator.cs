namespace GDFrameworkExtend.ResKit
{
    public interface IResCreator
    {
        bool Match(ResSearchKeys resSearchKeys);
        IRes Create(ResSearchKeys resSearchKeys);
    }
}