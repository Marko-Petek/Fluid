#  Tested with Python 3.7.1, Marko Petek, 12. II. 2019
#%% 
import numpy as NP
import matplotlib.pyplot as Plt
import matplotlib.lines as Lines
from mpl_toolkits.mplot3d import Axes3D
from matplotlib.ticker import LinearLocator, FormatStrFormatter

def Transpose(listInst):
    return list(map(list, zip(*listInst)))

def CreateLineFromNodes(nodes):
    nodesT = Transpose(nodes)
    xCoords = nodesT[0]
    yCoords = nodesT[1]
    return Lines.Line2D(xCoords, yCoords)
#%%  We choose nodes on a 3x3 rectangular domain.
nodes = [                                           # Red node coordinates.
    [  0,100], [ 45,100], [ 75,100], [100,100],
    [  0, 85], [ 30, 80], [ 80, 75], [100, 70],
    [  0, 35], [ 35, 40], [ 75, 35], [100, 40],
    [  0,  0], [ 40,  0], [ 80,  0], [100,  0]
]

borderNodes = [                                     # Blue border line.
    nodes[0], nodes[1], nodes[2], nodes[3],
    nodes[7], nodes[11], nodes[15],
    nodes[14], nodes[13], nodes[12],
    nodes[4], nodes[8], nodes[0]
]

vLine1Nodes = [
    nodes[1], nodes[5], nodes[9], nodes[13]
]

vLine2Nodes = [
    nodes[2], nodes[6], nodes[10], nodes[14]
]

hLine1Nodes = [
    nodes[4], nodes[5], nodes[6], nodes[7]
]

hLine2Nodes = [
    nodes[8], nodes[9], nodes[10], nodes[11]
]

nodesT = Transpose(nodes)                           # Transpose nodes array.
fig = Plt.figure(num = 0, figsize = (6,6), dpi = 90)
axes = fig.gca(xlim = (-1, 101), ylim = (-1, 101))
axes.plot(nodesT[0], nodesT[1], "ro")
axes.add_line(CreateLineFromNodes(borderNodes))         # Add border line.
axes.add_line(CreateLineFromNodes(vLine1Nodes))
axes.add_line(CreateLineFromNodes(vLine2Nodes))
axes.add_line(CreateLineFromNodes(hLine1Nodes))
axes.add_line(CreateLineFromNodes(hLine2Nodes))
#%%

#%%
def Gaussian(x, y, x0, y0, std):
    return 1.0 / (2 * np.pi * std**2) * np.e**(-((x-x0)**2 + (y-y0)**2) / (2 * std**2))

# Adds a wireframe plot of a single gaussian node function to a 3D plot with (x,y)
# meshgrid domain.
def AddGaussianToPlot(ax, x, y, x0, y0, std, stride, color):
    ax.plot_wireframe(x, y, Gaussian(x, y, x0, y0, std), rstride = stride, cstride = stride,
    color = color, alpha = 0.5, antialiased = True)

def AddGaussiansToPlot(ax, x, y, x0s, y0s, stds, strides, colors):
    for i in range(len(x0s)):
        AddGaussianToPlot(ax, x, y, x0s[i], y0s[i], stds[i], strides[i], colors[i])

# class Gaussian:
#     def __init__():
#         pass
#%%
fig = plt.figure(num = 0, figsize = (6,5), dpi = 72)
ax = fig.gca(projection = '3d', xlim = (0, 100), ylim = (0, 100))
ax.xaxis.set_major_locator(LinearLocator(0))
ax.yaxis.set_major_locator(LinearLocator(0))
ax.zaxis.set_major_locator(LinearLocator(0))
# x = np.arange(0, 100, 0.25)
# y = np.arange(0, 100, 0.25)
# (x, y) = np.meshgrid(x, y)

x1 = np.arange(25, 75, 0.25)
y1 = np.arange(25, 75, 0.25)
(x1, y1) = np.meshgrid(x1, y1)
x2 = np.arange(30, 70, 0.25)
y2 = np.arange(50, 100, 0.25)
(x2, y2) = np.meshgrid(x2, y2)

x0s = [50, 50]                           # X coords of node functions.
y0s = [50, 75]                           # Y coords of node functions.
stds = [10.0, 6.0]                       # Standard deviations of node functions.
colors = ['red', 'blue', 'green']
strides = [20, 10]
AddGaussianToPlot(ax, x1, y1, x0s[0], y0s[0], stds[0], strides[0], colors[0])
AddGaussianToPlot(ax, x2, y2, x0s[1], y0s[1], stds[1], strides[1], colors[1])
#AddGaussiansToPlot(ax, x, y, x0s, y0s, stds, strides, colors)

#surf = ax.plot_wireframe(x, y, Gaussian(x0[0], y0[0], std[0]), rstride = 20, cstride = 20, color = 'red', alpha = 0.5, antialiased = True)
