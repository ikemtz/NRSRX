using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using IkeMtz.NRSRx.Core.Unigration;
using IkeMtz.NRSRx.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IkeMtz.NRSRx.Core.Tests
{
  [TestClass]
  public class EventsTests : BaseUnigrationTests
  {
    [TestMethod]
    [TestCategory("Unit")]
    public void ValidateEvents()
    {
      var assembly = typeof(CreatedEvent).Assembly;
      var eventTypes = assembly.GetTypes()
         .Where(t => t.BaseType == typeof(EventType))
         .Select(t => t.GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<Object>()) as EventType)
         .Select(t => t.EventSuffix)
         .ToList();
      CollectionAssert.AllItemsAreUnique(eventTypes);
    }
  }
}
