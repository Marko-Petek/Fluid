#  Tested with Python 3.7.1, Marko Petek, 12. II. 2019
#%% 
import numpy as np
import matplotlib.pyplot as Plt
import matplotlib.lines as Lines
from mpl_toolkits.mplot3d import Axes3D
from matplotlib import cm
from matplotlib.ticker import LinearLocator, FormatStrFormatter
from array import array

def Gaussian(x, y, x0, y0, std):
    return np.e**(-((x-x0)**2 + (y-y0)**2) / (2 * std**2))
    #return 1.0 / (2 * np.pi * std**2) * np.e**(-((x-x0)**2 + (y-y0)**2) / (2 * std**2))

def Transpose(listInst):
    return list(map(list, zip(*listInst)))

def CreateLineFromNodes(nodes):
    nodesT = Transpose(nodes)
    xCoords = nodesT[0]
    yCoords = nodesT[1]
    return Lines.Line2D(xCoords, yCoords)
#%%  We choose nodes on a 3x3 rectangular domain.
nodes = [                                                           # Red node coordinates.
    [  0,100], [ 45,100], [ 75,100], [100,100],
    [  0, 85], [ 45, 60], [ 65, 65], [100, 70],
    [  0, 35], [ 35, 35], [ 70, 30], [100, 40],
    [  0,  0], [ 40,  0], [ 80,  0], [100,  0]
]
borderNodes = [                                                     # Blue border line.
    nodes[0], nodes[1], nodes[2], nodes[3],
    nodes[7], nodes[11], nodes[15],
    nodes[14], nodes[13], nodes[12],
    nodes[4], nodes[8], nodes[0]
]
vLine1Nodes = [nodes[1], nodes[5], nodes[9], nodes[13]]
vLine2Nodes = [nodes[2], nodes[6], nodes[10], nodes[14]]
hLine1Nodes = [ nodes[4], nodes[5], nodes[6], nodes[7]]
hLine2Nodes = [nodes[8], nodes[9], nodes[10], nodes[11]]

nodesT = Transpose(nodes)                                           # Transpose nodes array.
fig = Plt.figure(num = 0, figsize = (6,6), dpi = 90)
axes = fig.gca(xlim = (-1, 101), ylim = (-1, 101))
axes.plot(nodesT[0], nodesT[1], "ro")
axes.add_line(CreateLineFromNodes(borderNodes))                     # Add border line.
axes.add_line(CreateLineFromNodes(vLine1Nodes))
axes.add_line(CreateLineFromNodes(vLine2Nodes))
axes.add_line(CreateLineFromNodes(hLine1Nodes))
axes.add_line(CreateLineFromNodes(hLine2Nodes))
#%%
def AddGaussianToAxes(ax, x, y, x0, y0, std, stride, cmap):
    """Adds a wireframe plot of a single gaussian node function to a 3D plot with (x,y) meshgrid domain."""
    z = np.empty([400, 400])

    for i in range(400):
        for j in range(400):
            z[i][j] = Gaussian(x[i][j], y[i][j], x0, y0, std)

    ax.plot_surface(x, y, z, #rstride = stride, cstride = stride,
    cmap = cmap, vmin = -0.6, vmax = 1.0, alpha = 0.85, antialiased = True)

def AddGaussiansToAxes(ax, x, y, x0s, y0s, stds, strides, cmaps):
    for i in range(len(x0s)):
        AddGaussianToAxes(ax, x, y, x0s[i], y0s[i], stds[i], strides[i], cmaps[i])

#%%
fig = Plt.figure(num = 0, figsize = (6, 6), dpi = 72)
axes3d = fig.gca(projection = '3d', xlim = (0, 100), ylim = (0, 100))
axes3d.set_proj_type('ortho')
axes3d.view_init(40, 45)
axes3d.xaxis.set_major_locator(LinearLocator(5))                        # Remove ticks.
axes3d.yaxis.set_major_locator(LinearLocator(5))
axes3d.zaxis.set_major_locator(LinearLocator(3))
x = np.arange(0, 100, 0.25)
y = np.arange(0, 100, 0.25)
(x, y) = np.meshgrid(x, y)
centralNodes = [nodes[5], nodes[6], nodes[9], nodes[10]]                # Plot Gaussians on top of four middle nodes.
centralNodesT = Transpose(centralNodes)
stds = [20.0, 12.0, 16.0, 24.0]                                                  # Standard deviations of node functions.
strides = [5, 3, 4, 6]
cmaps = [cm.get_cmap('Blues', 20), cm.get_cmap('Reds', 20), cm.get_cmap('Greens', 20), cm.get_cmap('Purples', 20)]
AddGaussiansToAxes(axes3d, x, y, centralNodesT[0], centralNodesT[1], stds, strides, cmaps)
