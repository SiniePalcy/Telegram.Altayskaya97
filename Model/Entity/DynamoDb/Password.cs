﻿using Amazon.DynamoDBv2.DataModel;
using System;
using Telegram.Altayskaya97.Core.Interface;
using Telegram.Altayskaya97.Core.Model;

namespace Telegram.Altayskaya97.Model.Entity.DynamoDb
{
#if DEBUG
    [DynamoDBTable("Password" + "Test")]
#else
    [DynamoDBTable("Password")]
#endif
    public class Password : IObject
    {
        [DynamoDBHashKey]
        public virtual long Id { get; set; }
        [DynamoDBProperty]
        public virtual string ChatType { get; set; }
        [DynamoDBProperty]
        public virtual byte[] Hash { get; set; }
    }
}
