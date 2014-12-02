function calculatePercents(lists, list) {
    if (!list.ItemCount)
        return "0 %";

    var sum = 0;
    lists.forEach(function (l) { sum += l.ItemCount; });

    return Math.round(sum / list.ItemCount) + " %";
}