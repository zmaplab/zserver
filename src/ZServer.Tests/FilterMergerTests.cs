using ZMap;
using Xunit;

namespace ZServer.Tests;

public class FilterMergerTests
{
    private const string DefaultFilterJson = """
        {
          "Logic": "And",
          "Filters": [
            {
              "Field": "status",
              "Operator": "Equal",
              "Value": 1
            }
          ]
        }
        """;

    [Fact]
    public void Both_null_returns_null()
    {
        var result = FilterMerger.Merge(null, null);
        Assert.Null(result);
    }

    [Fact]
    public void Empty_strings_returns_null()
    {
        var result = FilterMerger.Merge("", "  ");
        Assert.Null(result);
    }

    [Fact]
    public void Only_default_filter()
    {
        var result = FilterMerger.Merge(DefaultFilterJson, null);
        Assert.Contains("\"status\"", result);
        Assert.Contains("\"Equal\"", result);
    }

    [Fact]
    public void Only_request_filter()
    {
        var requestFilter = """
            {
              "Logic": "And",
              "Filters": [
                {
                  "Field": "type",
                  "Operator": "Equal",
                  "Value": "building"
                }
              ]
            }
            """;

        var result = FilterMerger.Merge(null, requestFilter);
        Assert.Contains("\"type\"", result);
        Assert.Contains("\"building\"", result);
    }

    [Fact]
    public void Merge_different_fields_keeps_both()
    {
        var requestFilter = """
            {
              "Logic": "And",
              "Filters": [
                {
                  "Field": "type",
                  "Operator": "Equal",
                  "Value": "building"
                }
              ]
            }
            """;

        var result = FilterMerger.Merge(DefaultFilterJson, requestFilter);

        Assert.Contains("\"status\"", result);
        Assert.Contains("\"type\"", result);
    }

    [Fact]
    public void Merge_same_field_request_wins()
    {
        var requestFilter = """
            {
              "Logic": "And",
              "Filters": [
                {
                  "Field": "status",
                  "Operator": "Equal",
                  "Value": 2
                }
              ]
            }
            """;

        var result = FilterMerger.Merge(DefaultFilterJson, requestFilter);

        Assert.Contains("\"status\"", result);
        Assert.Contains("2", result);
        Assert.DoesNotContain("\"Value\":1", result);
    }

    [Fact]
    public void Merge_default_has_two_fields_request_overrides_one()
    {
        var defaultFilter = """
            {
              "Logic": "And",
              "Filters": [
                {
                  "Field": "status",
                  "Operator": "Equal",
                  "Value": 1
                },
                {
                  "Field": "category",
                  "Operator": "Equal",
                  "Value": "A"
                }
              ]
            }
            """;

        var requestFilter = """
            {
              "Logic": "And",
              "Filters": [
                {
                  "Field": "status",
                  "Operator": "GreaterThan",
                  "Value": 5
                }
              ]
            }
            """;

        var result = FilterMerger.Merge(defaultFilter, requestFilter);

        Assert.Contains("\"category\"", result);
        Assert.Contains("\"A\"", result);
        Assert.Contains("\"status\"", result);
        Assert.Contains("5", result);
    }

    [Fact]
    public void Merge_result_logic_is_always_and()
    {
        var requestFilter = """
            {
              "Logic": "Or",
              "Filters": [
                {
                  "Field": "type",
                  "Operator": "Equal",
                  "Value": "road"
                }
              ]
            }
            """;

        var result = FilterMerger.Merge(DefaultFilterJson, requestFilter);

        Assert.Contains("\"Logic\":\"And\"", result);
    }

    [Fact]
    public void Merge_with_empty_default()
    {
        var requestFilter = """
            {
              "Logic": "And",
              "Filters": [
                {
                  "Field": "type",
                  "Operator": "Equal",
                  "Value": "building"
                }
              ]
            }
            """;

        var result = FilterMerger.Merge("", requestFilter);
        Assert.Contains("\"type\"", result);
    }

    [Fact]
    public void Merge_with_empty_request()
    {
        var result = FilterMerger.Merge(DefaultFilterJson, "");
        Assert.Contains("\"status\"", result);
    }

    [Fact]
    public void Merge_multiple_fields_all_overridden()
    {
        var defaultFilter = """
            {
              "Logic": "And",
              "Filters": [
                {
                  "Field": "status",
                  "Operator": "Equal",
                  "Value": 1
                },
                {
                  "Field": "category",
                  "Operator": "Equal",
                  "Value": "A"
                }
              ]
            }
            """;

        var requestFilter = """
            {
              "Logic": "And",
              "Filters": [
                {
                  "Field": "status",
                  "Operator": "Equal",
                  "Value": 99
                },
                {
                  "Field": "category",
                  "Operator": "Equal",
                  "Value": "Z"
                }
              ]
            }
            """;

        var result = FilterMerger.Merge(defaultFilter, requestFilter);

        Assert.Contains("\"status\"", result);
        Assert.Contains("99", result);
        Assert.Contains("\"category\"", result);
        Assert.Contains("\"Z\"", result);
        Assert.DoesNotContain("\"Value\":1", result);
        Assert.DoesNotContain("\"A\"", result);
    }

    // === 新增边界条件测试 ===

    [Fact]
    public void Both_filters_with_empty_arrays_returns_null()
    {
        var defaultFilter = """{"Logic":"And","Filters":[]}""";
        var requestFilter = """{"Logic":"And","Filters":[]}""";

        var result = FilterMerger.Merge(defaultFilter, requestFilter);
        Assert.Null(result);
    }

    [Fact]
    public void Default_filter_with_empty_array_and_valid_request()
    {
        var defaultFilter = """{"Logic":"And","Filters":[]}""";
        var requestFilter = """
            {
              "Logic": "And",
              "Filters": [
                {
                  "Field": "type",
                  "Operator": "Equal",
                  "Value": "building"
                }
              ]
            }
            """;

        var result = FilterMerger.Merge(defaultFilter, requestFilter);
        Assert.Contains("\"type\"", result);
    }

    [Fact]
    public void Valid_default_and_request_with_empty_array()
    {
        var requestFilter = """{"Logic":"And","Filters":[]}""";

        var result = FilterMerger.Merge(DefaultFilterJson, requestFilter);
        Assert.Contains("\"status\"", result);
    }

    [Fact]
    public void Invalid_default_filter_json_throws()
    {
        var invalidDefault = "not valid json {{{";
        var requestFilter = """
            {
              "Logic": "And",
              "Filters": [
                {
                  "Field": "type",
                  "Operator": "Equal",
                  "Value": "building"
                }
              ]
            }
            """;

        Assert.ThrowsAny<Newtonsoft.Json.JsonException>(() =>
            FilterMerger.Merge(invalidDefault, requestFilter));
    }

    [Fact]
    public void Invalid_request_filter_json_throws()
    {
        var invalidRequest = "broken json <<<";
        Assert.ThrowsAny<Newtonsoft.Json.JsonException>(() =>
            FilterMerger.Merge(DefaultFilterJson, invalidRequest));
    }

    [Fact]
    public void Both_invalid_json_throws()
    {
        Assert.ThrowsAny<Newtonsoft.Json.JsonException>(() =>
            FilterMerger.Merge("bad{", "also bad}"));
    }

    [Fact]
    public void Filter_without_Filters_property_treated_as_empty()
    {
        var defaultFilter = """{"Logic":"And"}""";
        var requestFilter = """
            {
              "Logic": "And",
              "Filters": [
                {
                  "Field": "type",
                  "Operator": "Equal",
                  "Value": "road"
                }
              ]
            }
            """;

        var result = FilterMerger.Merge(defaultFilter, requestFilter);
        Assert.Contains("\"type\"", result);
    }

    [Fact]
    public void Filter_item_without_field_is_preserved()
    {
        // 没有 Field 的 filter 条件（如嵌套子条件）不应被去重逻辑干扰
        var defaultFilter = """
            {
              "Logic": "And",
              "Filters": [
                {
                  "Logic": "Or",
                  "Filters": [
                    {"Field": "a", "Operator": "Equal", "Value": 1},
                    {"Field": "b", "Operator": "Equal", "Value": 2}
                  ]
                }
              ]
            }
            """;
        var requestFilter = """
            {
              "Logic": "And",
              "Filters": [
                {
                  "Field": "c",
                  "Operator": "Equal",
                  "Value": 3
                }
              ]
            }
            """;

        var result = FilterMerger.Merge(defaultFilter, requestFilter);
        Assert.Contains("\"a\"", result);
        Assert.Contains("\"b\"", result);
        Assert.Contains("\"c\"", result);
    }
}
