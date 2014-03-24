namespace ObjectDump
{
   using System;
   using System.Collections;
   using System.Collections.Generic;
   using System.Linq;
   using System.Reflection;

   /// <summary>
   /// Stores dump information for a single object.
   /// </summary>
   public class ObjectDump : IObjectDump
   {
      #region fields

      /// <summary>
      /// Indicates whether to dump properties.
      /// </summary>
      private readonly bool _dumpProperties;

      /// <summary>
      /// Indicates whether to dump enumerable members.
      /// </summary>
      private readonly bool _dumpEnumerableMembers;

      #endregion

      #region constructor

      /// <summary>
      /// Initializes a new instance of the <see cref="ObjectDump" /> class.
      /// </summary>
      /// <param name="valueToDump">The object to dump.</param>
      /// <param name="declaredType">The declared type of the dumped object.</param>
      /// <param name="name">Name of the value being dumped.</param>
      /// <param name="dumpProperties">
      /// Indicates whether to dump properties.
      /// Take into consideration that property getters may cause side-effects when being evaluated!
      /// </param>
      /// <param name="dumpEnumerableMembers">
      /// Indicates whether to dump enumerable members.
      /// Take into consideration that enumerating may cause side-effects!
      /// </param>
      public ObjectDump(
         object valueToDump,
         Type declaredType,
         string name = "Root",
         bool dumpProperties = false,
         bool dumpEnumerableMembers = false)
      {
         _dumpProperties = dumpProperties;
         _dumpEnumerableMembers = dumpEnumerableMembers;

         RawObject = valueToDump;
         DeclaredType = declaredType;
         ObjectName = name;

         if (valueToDump != null)
         {
            ActualType = valueToDump.GetType();
            ObjectValue = CalculateDescription();
            PublicFields = CalculatePublicFields();
            Properties = _dumpProperties ? CalculateProperties() : Enumerable.Empty<IObjectDump>();
            EnumerableMembers = _dumpEnumerableMembers ? CalculateEnumerableMembers() : Enumerable.Empty<IObjectDump>();
         }
         else
         {
            ActualType = typeof(void);
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
      /// Gets the declaration type of the object.
      /// </summary>
      public Type DeclaredType { get; private set; }

      /// <summary>
      /// Gets the actual type of the object.
      /// </summary>
      public Type ActualType { get; private set; }

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
         string description;

         // take into account .ToString() overrides that throw an exception
         try
         {
            description = RawObject.ToString();
         }
         catch (Exception ex)
         {
            description = "<Exception>: " + ex;
         }

         return description;
      }

      /// <summary>
      /// Returns a lazily generated collection that contains dumps of the public fields.
      /// </summary>
      /// <returns>Dumps of all public fields.</returns>
      private IEnumerable<IObjectDump> CalculatePublicFields()
      {
         // get all public fields of the instance
         return ActualType.GetMembers(BindingFlags.Instance | BindingFlags.Public)
                          .OfType<FieldInfo>()
                          .Select(field => new ObjectDump(
                                     field.GetValue(RawObject),
                                     field.FieldType,
                                     field.Name,
                                     _dumpProperties,
                                     _dumpEnumerableMembers))
                          .OrderBy(dump => dump.ObjectName);
      }

      /// <summary>
      /// Returns a lazily generated collection that contains dumps of the public (non-indexed) properties.
      /// </summary>
      /// <returns>Dumps of all properties.</returns>
      private IEnumerable<IObjectDump> CalculateProperties()
      {
         // get all public properties that are not indexed
         return ActualType.GetMembers(BindingFlags.Instance | BindingFlags.Public)
                          .OfType<PropertyInfo>()
                          .Where(property => !property.GetIndexParameters().Any())
                          .Select(property => new ObjectDump(
                                     GetSafePropertyValue(property),
                                     property.PropertyType,
                                     property.Name,
                                     _dumpProperties,
                                     _dumpEnumerableMembers))
                          .OrderBy(dump => dump.ObjectName);
      }

      /// <summary>
      /// Gets the value of the given property and avoids possible exceptions.
      /// </summary>
      /// <param name="property">The property.</param>
      /// <returns>The value of the property if retrievable.</returns>
      private object GetSafePropertyValue(PropertyInfo property)
      {
         object propertyValue;

         // take into account properties that throw an exception
         try
         {
            propertyValue = property.GetValue(RawObject, null);
         }
         catch (Exception ex)
         {
            // return the thrown exception as the actual value
            propertyValue = ex;
         }

         return propertyValue;
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
               yield return new ObjectDump(member, enumeratedType, "[" + index++ + "]", _dumpProperties, _dumpEnumerableMembers);
            }
         }
      }

      /// <summary>
      /// Gets the first type that is implemented via IEnumerable of T.
      /// </summary>
      /// <returns>The type T of the first IEnumerable of T implementation if present, null otherwise.</returns>
      private Type GetGenericEnumeratedType()
      {
         return ActualType.GetInterfaces()
                          .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                          .Select(t => t.GetGenericArguments()[0])
                          .FirstOrDefault();
      }

      #endregion
   }
}