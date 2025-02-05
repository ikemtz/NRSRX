namespace IkeMtz.NRSRx.Jobs.Core
{
  /// <summary>
  /// Represents metadata for a function in the NRSRx framework.
  /// </summary>
  public class FunctionMetadata
  {
    /// <summary>
    /// Gets or sets the type of the function.
    /// </summary>
    public Type Type { get; set; }

    /// <summary>
    /// Gets or sets the name of the function.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the sequence priority of the function.
    /// </summary>
    public int SequencePriority { get; set; }
  }
}
