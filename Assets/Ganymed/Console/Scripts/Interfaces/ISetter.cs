namespace Ganymed.Console
{
    public interface ISetter
    {   
        /// <summary>
        /// Determines a default value for the auto-complete feature of the console.
        /// </summary>
        object Default { get; set; }
    }
}
 