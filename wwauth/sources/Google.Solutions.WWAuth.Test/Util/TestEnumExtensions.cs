//
// Copyright 2022 Google LLC
//
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
//

using Google.Solutions.WWAuth.Util;
using NUnit.Framework;

namespace Google.Solutions.WWAuth.Test.Util
{
    [TestFixture]
    public class TestEnumExtensions
    {
        //---------------------------------------------------------------------
        // GetDescription.
        //---------------------------------------------------------------------

        private enum Toppings
        {
            [System.ComponentModel.Description("Milky substance")]
            Cream,

            Chocolate
        }

        [Test]
        public void WhenDescriptionAttributeSet_ThenGetDescriptionReturnsDescription()
        {
            Assert.AreEqual("Milky substance", Toppings.Cream.GetDescription());
        }

        [Test]
        public void WhenDescriptionAttributeNotSet_ThenGetDescriptionReturnsName()
        {
            Assert.AreEqual("Chocolate", Toppings.Chocolate.GetDescription());
        }

        //---------------------------------------------------------------------
        // FromDescription.
        //---------------------------------------------------------------------

        [Test]
        public void WhenDescriptionFound_ThenFromDescriptionReturnsValue()
        {
            var topping = EnumExtensions.FromDescription<Toppings>("Milky substance");
            Assert.AreEqual(Toppings.Cream, topping);
        }

        [Test]
        public void WhenDescriptionNotFound_ThenFromDescriptionReturnsNull()
        {
            var topping = EnumExtensions.FromDescription<Toppings>("Slimy substance");
            Assert.IsNull(topping);
        }
    }
}
