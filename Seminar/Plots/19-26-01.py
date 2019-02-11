#%%
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D
from matplotlib import cm
from matplotlib.ticker import LinearLocator, FormatStrFormatter
import numpy as np

def Gaussian(x0, y0, std):
    return 1.0 / (2 * np.pi * std**2) * np.e**(-((x-x0)**2 + (y-y0)**2) / (2 * std**2))

def CreateWireFrame(func):
    
#%%
fig = plt.figure(0, (6,5), 72)
ax = fig.gca(projection = '3d')
ax.xaxis.set_major_locator(LinearLocator(0))
ax.yaxis.set_major_locator(LinearLocator(0))
ax.zaxis.set_major_locator(LinearLocator(0))
x = np.arange(0, 100, 0.25)
y = np.arange(0, 100, 0.25)
(x, y) = np.meshgrid(x, y)

x0 = [50, 75]                           # X coords of node functions.
y0 = [50, 50]                           # Y coords of node functions.
std = [10.0, 6.0]                       # Standard deviations of node functions.

surf = ax.plot_wireframe(x, y, Gaussian(x0[0], y0[0], std[0]), rstride = 20, cstride = 20, color = 'red', alpha = 0.5, antialiased = True)
