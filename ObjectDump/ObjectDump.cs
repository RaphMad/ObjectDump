namespace ObjectDump
{
   using System;
   using System.Collections;
   using System.Collections.Generic;
   using System.Linq;
   using System.Reflection;

   /// <summary>
   /// Contains information about the data contained in an object.
   /// </summary>
   public class ObjectDump : IObjectDump
   {
      #region constructor

      /// <summary>
      /// Initializes a new instance of the <see cref="ObjectDump" /> class.
      /// </summary>
      /// <param name="valueToDump">The value to dump.</param>
      /// <param name="name">Name of the value being dumped.</param>
      /// <param name="declarationType">
      /// The declaration type of the value. 
      /// In fact only needed to have a type for "null" objects, for non-null objects the actual type will be dynamically determined.
      /// </param>
      public ObjectDump(object valueToDump, string name, Type declarationType)
      {
         RawObject = valueToDump;
         ObjectName = name;

         if (valueToDump != null)
         {
            // dynamically determine the actual type
            ObjectType = valueToDump.GetType();
            ObjectValue = CalculateDescription();
            PublicFields = CalculatePublicFields();
            Properties = CalculateProperties();
            EnumerableMembers = CalculateEnumerableMembers();
         }
         else
         {
            // object to dump is null - at least we can use its declared type
            ObjectType = declarationType;
            ObjectValue = "<null>";
            PublicFields = Enumerable.Empty<IObjectDump>();
            Properties = Enumerable.Empty<IObjectDump>();
            EnumerableMembers = Enumerable.Empty<IObjectDump>();
         }
      }

      #endregion

      #region interface implementation

      /// <summary>
      /// Gets the raw object.
      /// </summary>
      public object RawObject { get; private set; }

      /// <summary>
      /// Gets the name of the object.
      /// </summary>
      public string ObjectName { get; private set; }

      /// <summary>
      /// Gets the type of the object.
      /// </summary>
      public Type ObjectType { get; private set; }

      /// <summary>
      /// Gets a string describing the content of the object.
      /// </summary>
      public string ObjectValue { get; private set; }

      /// <summary>
      /// Gets dumps of the public fields of the object.
      /// </summary>
      public IEnumerable<IObjectDump> PublicFields { get; private set; }

      /// <summary>
      /// Gets dumps of the properties of the object.
      /// </summary>
      public IEnumerable<IObjectDump> Properties { get; private set; }

      /// <summary>
      /// Gets dumps of the enumerable members of the object.
      /// </summary>
      public IEnumerable<IObjectDump> EnumerableMembers { get; private set; }

      #endregion

      #region private methods

      /// <summary>
      /// Calculates a description for the dumped object.
      /// </summary>
      /// <returns>A descriptive string.</returns>
      private string CalculateDescription()
      {
         return RawObject.ToString();
      }

      /// <summary>
      /// Returns a lazily generated collection that contains dumps of the public fields.
      /// </summary>
      /// <returns>Dumps of all public fields.</returns>
      private IEnumerable<IObjectDump> CalculatePublicFields()
      {
         // get all public fields of the instance
         return ObjectType.GetMembers(BindingFlags.Instance | BindingFlags.Public)
                          .OfType<FieldInfo>()
                          .Select(field => new ObjectDump(field.GetValue(RawObject), field.Name, field.FieldType))
                          .OrderBy(dump => dump.ObjectName);
      }

      /// <summary>
      /// Returns a lazily generated collection that contains dumps of the public (non-indexed) properties.
      /// </summary>
      /// <returns>Dumps of all properties.</returns>
      private IEnumerable<IObjectDump> CalculateProperties()
      {
         // get all public properties that are not indexed
         return ObjectType.GetMembers(BindingFlags.Instance | BindingFlags.Public)
                          .OfType<PropertyInfo>()
                          .Where(property => !property.GetIndexParameters().Any())
                          .Select(property => new ObjectDump(property.GetValue(RawObject, null), property.Name, property.PropertyType))
                          .OrderBy(dump => dump.ObjectName);
      }

      /// <summary>
      /// Returns a lazily generated collection that contains dumps of the enumerable members.
      /// </summary>
      /// <returns>Dumps of all enumerable members.</returns>
      private IEnumerable<IObjectDump> CalculateEnumerableMembers()
      {
         var enumeration = RawObject as IEnumerable;

         if (enumeration != null)
         {
            // use the "T" of IEnumerable<T> as declaration type if possible, "typeof(object)" as a fallback (in case of non-generic IEnumerable)
            Type enumeratedType = GetGenericEnumeratedType() ?? typeof(object);

            // enumerable members will be named by their index: [<index>]
            int index = 0;

            foreach (object member in enumeration)
            {
               yield return new ObjectDump(member, "[" + index++ + "]", enumeratedType);
            }
         }
      }

      /// <summary>
      /// Gets the first type that is implemented via IEnumerable of T.
      /// </summary>
      /// <returns>The type T of the first IEnumerable of T implementation if present, null otherwise.</returns>
      private Type GetGenericEnumeratedType()
      {
         return ObjectType.GetInterfaces()
                          .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                          .Select(t => t.GetGenericArguments()[0])
                          .FirstOrDefault();
      }

      #endregion
   }
}