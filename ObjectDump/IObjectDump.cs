namespace ObjectDump
{
   using System;
   using System.Collections.Generic;

   /// <summary>
   /// Stores dump information for a single object.
   /// </summary>
   public interface IObjectDump
   {
      /// <summary>
      /// Gets the raw object.
      /// </summary>
      object RawObject { get; }

      /// <summary>
      /// Gets the name of the object.
      /// </summary>
      string ObjectName { get; }

      /// <summary>
      /// Gets the declared type of the object.
      /// </summary>
      Type DeclaredType { get; }

      /// <summary>
      /// Gets the actual type of the object.
      /// </summary>
      Type ActualType { get; }

      /// <summary>
      /// Gets a string describing the content of the object.
      /// </summary>
      string ObjectValue { get; }

      /// <summary>
      /// Gets dumps of the public fields of the object.
      /// </summary>
      IEnumerable<IObjectDump> PublicFields { get; }

      /// <summary>
      /// Gets dumps of the properties of the object.
      /// </summary>
      IEnumerable<IObjectDump> Properties { get; }

      /// <summary>
      /// Gets dumps of the enumerable members of the object.
      /// </summary>
      IEnumerable<IObjectDump> EnumerableMembers { get; }
   }
}