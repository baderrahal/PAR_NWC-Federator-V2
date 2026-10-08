# Q133 on 1A04PK, 2026-10-07. Reads a result of probe-q133-import.ps1 and, for every test where
# the original and its swap differ, pairs each clash only the original finds with a clash only
# the swap finds when the two share one item and the other two items are children of one parent,
# their index paths equal but for the last number. What is left is then paired a second way, called
# near: two clashes at the same distance, to 1e-6 ft, whose items are each the same or of one path
# length differing in exactly one number. Read only. Run as
#
#   python q133-sibling-check.py <result.txt> <out.txt>
#
# It writes one line per differing test, then the totals: clashes only one side finds, how many of
# them pair up as siblings, how many more pair up as near, and how many are left over.

import re
import sys

LINE = re.compile(r'LINE (\d+)  "([^"]+)".*?original (\d+) in .*?swap (\d+) in .*?, (same|swap finds more|swap finds fewer|other clashes|UNKNOWN)$')
ONLY = re.compile(r'only in the (original|swap): (\S+) \| (\S+)   distance (\S+)')


def parent(path):
    return path.rsplit(".", 1)[0] if "." in path else ""


def siblings(a, b):
    # a and b are (first, second) unordered pairs. True when one item is shared and the
    # other two differ only in their last index.
    for x in (0, 1):
        for y in (0, 1):
            if a[x] == b[y]:
                p, q = a[1 - x], b[1 - y]
                if p != q and parent(p) == parent(q):
                    return True
    return False


def one_apart(p, q):
    a, b = p.split("."), q.split(".")
    return len(a) == len(b) and sum(1 for x, y in zip(a, b) if x != y) <= 1


def near(a, b):
    (pa, da), (pb, db) = a, b
    if abs(da - db) > 1e-6:
        return False
    return (one_apart(pa[0], pb[0]) and one_apart(pa[1], pb[1])) or (one_apart(pa[0], pb[1]) and one_apart(pa[1], pb[0]))


def main():
    src, out = sys.argv[1], sys.argv[2]
    tests = []
    current = None
    with open(src, encoding="utf-8") as f:
        for raw in f:
            line = raw.rstrip("\r\n")
            m = LINE.search(line)
            if m:
                current = {"index": int(m.group(1)), "name": m.group(2), "original": int(m.group(3)),
                           "swap": int(m.group(4)), "verdict": m.group(5), "orig": [], "sw": []}
                tests.append(current)
                continue
            m = ONLY.search(line)
            if m and current is not None:
                item = ((m.group(2), m.group(3)), float(m.group(4)))
                (current["orig"] if m.group(1) == "original" else current["sw"]).append(item)

    rows = []
    totals = {"tests": 0, "only_orig": 0, "only_swap": 0, "paired": 0, "near": 0, "left_orig": 0, "left_swap": 0}
    for t in tests:
        if t["verdict"] in ("same",) or (not t["orig"] and not t["sw"]):
            continue
        totals["tests"] += 1
        free = list(t["sw"])
        paired = 0
        left_orig = []
        for o in t["orig"]:
            hit = None
            for i, s in enumerate(free):
                if siblings(o[0], s[0]):
                    hit = i
                    break
            if hit is None:
                left_orig.append(o)
            else:
                paired += 1
                free.pop(hit)
        near_pairs = 0
        still_orig = []
        for o in left_orig:
            hit = None
            for i, s in enumerate(free):
                if near(o, s):
                    hit = i
                    break
            if hit is None:
                still_orig.append(o)
            else:
                near_pairs += 1
                free.pop(hit)
        left_orig = still_orig
        totals["only_orig"] += len(t["orig"])
        totals["only_swap"] += len(t["sw"])
        totals["paired"] += paired
        totals["near"] += near_pairs
        totals["left_orig"] += len(left_orig)
        totals["left_swap"] += len(free)
        rows.append("%d\t%s\toriginal %d\tswap %d\t%s\tonly original %d\tonly swap %d\tsibling pairs %d\tnear pairs %d\tleft only original %d\tleft only swap %d\tdistances left only swap %s\tleft only original %s" % (
            t["index"], t["name"], t["original"], t["swap"], t["verdict"], len(t["orig"]), len(t["sw"]), paired, near_pairs,
            len(left_orig), len(free), ",".join("%.3f" % s[1] for s in free) or "-", ",".join("%.3f" % o[1] for o in left_orig) or "-"))

    rows.append("# tests where the two differ %d, clashes only the original finds %d, only the swap finds %d, of which sibling pairs %d, near pairs %d, left only in the original %d, left only in the swap %d" % (
        totals["tests"], totals["only_orig"], totals["only_swap"], totals["paired"], totals["near"], totals["left_orig"], totals["left_swap"]))
    with open(out, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(rows) + "\n")
    print(rows[-1])


if __name__ == "__main__":
    main()
