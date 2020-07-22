/*
 * Created by: Miguel Angel Medina Pérez (miguelmedinaperez@gmail.com)
 * Created: 09/28/2016
 * Comments by: Miguel Angel Medina Pérez (miguelmedinaperez@gmail.com)
 */

namespace PRFramework.Core.Common
{
    /// <summary>
    ///     A simple vector interface.
    /// </summary>
    /// <remarks>
    ///     There are algorithms (e.g. k-means) which work with simple vectors; so, using these algorithms in any other program requires only to implement IVector interface.
    /// </remarks>
    public interface IVector
    {
        /// <summary>
        ///     Indexer to access the components of the vector.
        /// </summary>
        /// <param name="index">The index of the component to be accessed.</param>
        /// <returns>The value of the component.</returns>
        double this[int index]
        {
            get;
            set;
        }

        /// <summary>
        ///     The length of the vector.
        /// </summary>
        int Length { get; }
    }
}
