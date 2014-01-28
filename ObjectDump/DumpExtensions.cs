using System;

namespace ObjectDump
{
   /// <summary>
   /// Contains extension methods that generate object dumps.
   /// </summary>
   public static class DumpExtensions
   {
      /// <summary>
      /// Generates a dump of the source value.
      /// </summary>
      /// <param name="valueToDump">The value to dump.</param>
      /// <param name="name">The name to be used for the dumped object.</param>
      /// <returns>
      /// The generated object dump.
      /// </returns>
      public static IObjectDump Dump(this object valueToDump, string name)
      {
         if (valueToDump == null)
         {
            throw new ArgumentNullException("valueToDump");
         }

         return new ObjectDump(valueToDump, name, valueToDump.GetType());
      }
   }
}