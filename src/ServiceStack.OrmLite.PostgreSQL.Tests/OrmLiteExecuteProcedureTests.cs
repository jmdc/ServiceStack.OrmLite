﻿using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.Tests;
using ServiceStack.Text;
using IgnoreAttribute = NUnit.Framework.IgnoreAttribute;

namespace ServiceStack.OrmLite.PostgreSQL.Tests
{
    [TestFixture]
    public class OrmLiteExecuteProcedureTests : OrmLiteTestBase
    {
        public OrmLiteExecuteProcedureTests() : base(Dialect.PostgreSql) { }

        private const string Create = @"
            CREATE OR REPLACE FUNCTION f_service_stack(
                v_string_values text[],
                v_integer_values integer[]
            ) RETURNS BOOLEAN AS
            $BODY$
            BEGIN
                IF v_string_values[1] <> 'ServiceStack' THEN
                    RAISE EXCEPTION 'Unexpected value in string array[1] %', v_string_values[1];
                END IF;
                IF v_string_values[2] <> 'Thoughtfully Architected' THEN
                    RAISE EXCEPTION 'Unexpected value in string array[2] %', v_string_values[2];
                END IF;
                IF v_integer_values[1] <> 1 THEN
                    RAISE EXCEPTION 'Unexpected value in integer array[1] %', v_integer_values[1];
                END IF;
                IF v_integer_values[2] <> 2 THEN
                    RAISE EXCEPTION 'Unexpected value in integer array[2] %', v_integer_values[2];
                END IF;
                IF v_integer_values[3] <> 3 THEN
                    RAISE EXCEPTION 'Unexpected value in integer array[3] %', v_integer_values[3];
                END IF;
                RETURN TRUE;
            END;
            $BODY$
            LANGUAGE plpgsql VOLATILE COST 100;
            ";

        private const string Drop = "DROP FUNCTION f_service_stack(text[], integer[]);";

        [Alias("f_service_stack")]
        public class ServiceStackFunction
        {
            [CustomField("text[]")]
            public string[] StringValues { get; set; }
            [CustomField("integer[]")]
            public int[] IntegerValues { get; set; }
        }

        [Test]
        [Ignore("This test fails both on .NET and .NET Core")]
        public void Can_execute_stored_procedure_with_array_arguments()
        {
            using (var db = OpenDbConnection())
            {
                db.ExecuteSql(Create);
                db.GetLastSql().Print();

                db.ExecuteProcedure(new ServiceStackFunction
                {
                    StringValues = new[] { "ServiceStack", "Thoughtfully Architected" },
                    IntegerValues = new[] { 1, 2, 3 }
                });

                db.ExecuteSql(Drop);
            }
        }
    }
}
