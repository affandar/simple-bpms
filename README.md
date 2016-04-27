---
platform: windows, Android, iOS
device: desktop, phone, tablet
language: csharp, json
---

Device Management Query Sample Snippets
===
---

# Table of Contents

* [Introduction](#Introduction)
* [Device Queries](#DeviceQueries)
 * [Filter over a Device Property](#device-query-filter-deviceprop)
 * [Filter over a Device property (not-equals)](#device-query-filter-deviceprop-ne)
 * [Filter over a Service property](#device-query-filter-serviceprop)
 * [Filter over a Service property with aggregates](#device-query-filter-serviceprop-aggregates)
 * [Order-by](#device-query-order-by)

<a name="Introduction"></a>
# Introduction

**About this document**

This document contains sample JSON snippets for various device and job queries that can be performed via the Device Management public preview APIs. 

Note that for csharp the snippets have to be modified to escape all the double quote characters. This is as simple as pasting in the cs code file and replacing every double quote character with two double quotes instead of one. I.e, " -> ""

<a name="DeviceQueries"></a>
# Device Queries

<a name="device-query-filter-deviceprop"></a>
**Filter over a Device Property**

Get all devices where the FirmwareVersion Device property is set to 1.0

``` js
{
    "filter": {
        "property": {
            "name": "FirmwareVersion",
            "type": "device"
        },
        "value": "1.0",
        "comparisonOperator": "eq",
        "type": "comparison"
    }
}
```

<a name="device-query-filter-deviceprop-ne"></a>
**Filter over a Device property (not-equals)**

Get all devices where the FirmwareVersion Device property is **not** set to 1.0

```js
{
    "filter": {
        "property": {
            "name": "FirmwareVersion",
            "type": "device"
        },
        "value": "1.0",
        "comparisonOperator": "ne",
        "type": "comparison"
    }
}
```

<a name="device-query-filter-serviceprop"></a>
**Filter over a Service property**

Get all devices where CustomerId Service property is set to 123456

```js
{
    "filter": {
        "property": {
            "name": "CustomerId",
            "type": "service"
        },
        "value": "123456",
        "comparisonOperator": "eq",
        "type": "comparison"
    }
}
```

<a name="device-query-filter-serviceprop-aggregates"></a>
**Filter over a Service property with aggregates**

Group by the CustomerId Service prooperty and get sum of Weight Service property for all devices where CustomerId Service property is present. 

```js
{
    "filter": {
        "property": {
            "name": "CustomerId",
            "type": "service"
        },
        "value": null,
        "comparisonOperator": "ne",
        "type": "comparison"
    },
    "aggregate": {
        "keys": [
            {
                "name": "CustomerId",
                "type": "service"
            }
        ],
        "properties": [
            {
                "operator": "sum",
                "property": {
                    "name": "Weight",
                    "type": "service"
                },
                "columnName": "TotalWeight"
            }
        ]
    },
    "sort": []
}
```

<a name="device-query-order-by"></a>
**Order-by**

Return all devices ordered by the QoS Service property.

```js
{
    "sort": [
        {
            "property": {
                "name": "QoS",
                "type": "service"
            },
            "order": "asc"
        }
    ]
}
```

<a name="JobQueries"></a>
# Job Queries
