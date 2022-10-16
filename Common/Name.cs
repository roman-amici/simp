namespace Simp.Common
{
    public class Name
    {
        public string QualifiedName { get; private set; }
        public Name(string name)
        {
            QualifiedName = name;
        }
    }
}