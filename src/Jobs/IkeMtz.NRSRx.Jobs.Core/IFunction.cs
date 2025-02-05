namespace IkeMtz.NRSRx.Jobs.Core
{
  /// <summary>
  /// Represents a function that can be executed within the NRSRx framework.
  /// </summary>
  public interface IFunction
  {
    /// <summary>
    /// Gets the sequence priority of the function. Higher priority functions are run first (ordered by descending).
    /// </summary>
    int? SequencePriority { get; }

    /// <summary>
    /// Executes the function asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the function was successful.</returns>
    Task<bool> RunAsync();
  }
}
