module LibUiAdmin.Styles

open ReactXP.LegacyStyles

let styles = lazy (compile [
    "hack-forcefully-add-global-style" => []
])

addCss("""

table.la-table {
    border-spacing: 0px;
    min-width:      100%;
}

table.la-table td {
    padding:        20px 10px;
    border-bottom:  1px solid #ccc;
    color:          #666;
    vertical-align: middle;
}

table.la-table > tbody > tr:last-child > td {
    border-bottom-width: 0;
}

table.la-table td.la-td-nowrap {
    white-space: nowrap;
}

table.la-table td:first-child {
    padding-left: 30px;
}

table.la-table thead td {
    padding: 20px 10px;
}

table.la-table-keyvalue {
    border-spacing: 0px;
    max-width:      100%;
}

table.la-table-keyvalue > tbody > tr > td {
    padding:       13px 10px;
    border-bottom: 1px solid #ccc;
}

table.la-table-keyvalue > tbody > tr:last-child > td {
    border-bottom-width: 0;
}

table.la-table-keyvalue > tbody > tr > td:first-child {
    white-space:    nowrap;
    vertical-align: top;
    text-align:     right;
    color:          #666;
    padding-right:  40px;
    width:          200px;
}

"""
)
