# Network

A network is a flat specification of all bonds that exist between nodes. Bonds are not specified from the perspective of any of the nodes that participate in it, instead, a bond is a node on itself. Take three nodes with IDs 3, 5 and 7. Permutations of (distinct) node IDs give us the off-diagonal elements:

| p1 = 357 |  | p2 = 375 |  | p3 = 537 |  | p4 = 573 |  | p5 = 735 |  | p6 = 753 |
|:--------:|--|:--------:|--|:--------:|--|:--------:|--|:--------:|--|:--------:|
| (3, {0}) |  | (3, {0}) |  | (3, {1}) |  | (3, {2}) |  | (3, {1}) |  | (3, {2}) |
| (5, {1}) |  | (5, {2}) |  | (5, {0}) |  | (5, {0}) |  | (5, {2}) |  | (5, {1}) |
| (7, {2}) |  | (7, {1}) |  | (7, {2}) |  | (7, {1}) |  | (7, {0}) |  | (7, {0}) |

We shall call elements p1 -> 357, p2 -> 375, p3 -> 537 ..., **bonds**. We will specify tensors by specifying all of its bonds. Partially diagonal elements look like this:

|  p7 = 355   |  |  p8 = 535   |  |  p9 = 553   |
|:-----------:|--|:-----------:|--|:-----------:|
|  (3, {0})   |  |  (3, {1})   |  |  (3, {2})   |
| (5, {1, 2}) |  | (5, {0, 2}) |  | (5, {0, 1}) |

Fully diagonal elements:

|   p10 = 555    |
|:--------------:|
| (5, {0, 1, 2}) |

We call each positional designation within a bond a **connection**, so that 

## When digit order does not matter

We can represent the whole upper row (all the off-diagonal elements) as q1 and the partially diagonal elements as q2:

|    q1     |  |     q2    |
|:---------:|--|:---------:|
| (3, {-1}) |  | (3, {-1}) |
| (5, {-1}) |  | (5, {-2}) |
| (7, {-1}) |  |           |

Summary: Positive integers in the connection's list indicate that the connection is a P-connection and the integers themselves represent docking positions. A negative integer in the connection's list indicates that the connection is a C-connection and the integer itself represents multiplicity of appearance of that connection.