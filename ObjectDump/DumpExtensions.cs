using System;

namespace ObjectDump
{
   /// <summary>
   /// Contains extension methods that generate object dumps.
   /// </summary>
   public static class DumpExtensions
   {
      /// <summary>
      /// Generates a dump for an object.
      /// </summary>
      /// <param name="valueToDump">The object to dump.</param>
      /// <param name="name">Name of the object being dumped.</param>
      /// <param name="dumpProperties">
      /// Indicates whether to dump properties.
      /// Take into consideration that property getters may cause side-effects when being evaluated!
      /// </param>
      /// <param name="dumpEnumerableMembers">
      /// Indicates whether to dump enumerable members.
      /// Take into consideration that enumerating may cause side-effects!
      /// </param>
      /// <returns>
      /// The generated dump.
      /// </returns>
      public static IObjectDump Dump(this object valueToDump, string name, bool dumpProperties = false, bool dumpEnumerableMembers = false)
      {
         if (valueToDump == null)
         {
            throw new ArgumentNullException("valueToDump");
         }

         return new ObjectDump(valueToDump, valueToDump.GetType(), name, dumpProperties, dumpEnumerableMembers);
      }
   }
}