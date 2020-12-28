namespace Ganymed.Utils
{
    public delegate void VisibilityDelegate(Visibility was, Visibility now);
    
    public interface IVisible
    {
        Visibility VisibilityFlags { get;}
        void SetVisibility(Visibility visibility);
        event VisibilityDelegate OnVisibilityChanged;
    }
}