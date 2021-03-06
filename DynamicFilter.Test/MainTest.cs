using System.Collections.Generic;
using System.Linq;
using DynamicFilter.Main;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DynamicFilter.Test
{
    [TestClass]
    public class MainTest
    {
        private List<TestDomain> _listDomainObjects;
        private List<AnotherClass> _listAnotherClass;

        [TestInitialize]
        public void CreateObj()
        {
            _listDomainObjects = new List<TestDomain>() {
                new TestDomain(1, "number 1"),
                new TestDomain(2, "number 2"),
                new TestDomain(3, "person 3"),
                new TestDomain(4, "animal 4"),
                new TestDomain(5, "person 5")
            };

            _listAnotherClass = new List<AnotherClass>() {
                new AnotherClass("person 1"),
                new AnotherClass("person 2"),
                new AnotherClass("empty")
            };
        }
            
        [TestMethod]
        public void TestEqual()
        {
            //Filter TestDomain.Id = 3 - By name (faster)
            Filter filter = new Filter();
            filter.Add(nameof(TestDto.Id), FilterType.Equal, 3);

            //Transform the filterDto into a specified domain predicate
            var result = _listDomainObjects.Where(filter.CreateFilter<TestDomain>());
            Assert.IsTrue(result.Single().Id == 3);

            filter.Clear();

            //Filter TestDomain.Id = 2 - By typed expression (slower)
            filter.Add<TestDto>(t => t.Id, FilterType.Equal, 2);

            result = _listDomainObjects.Where(filter.CreateFilter<TestDomain>());
            Assert.IsTrue(result.Single().Id == 2);

            filter.Clear();

            //Filter TestDomain.Id = 1 - By index (equal is the default filter type)
            filter.Add(nameof(TestDto.Id), FilterType.Equal, 1);

            result = _listDomainObjects.Where(filter.CreateFilter<TestDomain>());
            Assert.IsTrue(result.Single().Id == 1);
        }

        [TestMethod]
        public void TestContains()
        {            
            Filter filter = new Filter();
            filter.Add("Description", FilterType.Contains, "person");
            
            var result = _listDomainObjects.Where(filter.CreateFilter<TestDomain>());
            Assert.IsTrue(result.Count() == 2);

            //Apply same filter in another class
            var anotherResult = _listAnotherClass.Where(filter.CreateFilter<AnotherClass>());
            Assert.IsTrue(anotherResult.Count() == 2);
        }

        [TestMethod]
        public void TestMultiple()
        {
            //Description contains "person" and Id <= 3
            Filter filter = new Filter();
            filter.Add("Description", FilterType.Contains, "person");
            filter.Add("Id", FilterType.LessThanOrEqual, 3);

            var result = _listDomainObjects.Where(filter.CreateFilter<TestDomain>());
            Assert.IsTrue(result.Count() == 1);
        }

        [TestMethod]
        public void TestSQL()
        {
            //Description contains "person" and Id <= 3
            Filter filter = new Filter();
            filter.Add("Description", FilterType.Contains, "person").Or("Id", FilterType.LessThanOrEqual, 3);
            filter.Or("Value", FilterType.GreaterThan, 1);

            var result = filter.CreateFilterSQL<TestDomain>();            
        }


        [TestMethod]
        public void TestOr()
        {            
            Filter filter = new Filter();
            filter.Add("Description", FilterType.Contains, "person").Or("Id", FilterType.LessThanOrEqual, 3);
            filter.Add("Description", FilterType.NotEqual, "number 1");
            filter.Or("Description", FilterType.Contains, "animal");
                         
            var result = _listDomainObjects.Where(filter.CreateFilter<TestDomain>());            

            Assert.IsTrue(result.Count() == 4);
        }

        private class TestDto
        {
            public int Id { get; set; }
            public string Description { get; set; }
            public decimal Value { get; set; }

            public TestDto(int id, string description)
            {
                Id = id;
                Description = description;
            }
        }

        private class TestDomain
        {
            public TestDomain()
            {

            }

            public int Id { get; set; }
            public string Description { get; set; }

            public decimal Value { get; set; }

            public TestDomain(int id, string description)
            {
                Id = id;
                Description = description;
            }
        }

        private class AnotherClass
        {            
            public string Description { get; set; }

            public AnotherClass(string description)
            {
                Description = description;
            }
        }
    }
}
