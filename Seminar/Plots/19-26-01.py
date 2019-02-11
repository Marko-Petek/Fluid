#%%
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D
from matplotlib import cm
from matplotlib.ticker import LinearLocator, FormatStrFormatter
import numpy as np
#%%
fig = plt.figure()
ax = fig.gca(projection = '3d')

X = np.arange(-5, 5, 0.25)
Y = np.arange(-5, 5, 0.25)
X, Y = np.meshgrid(X, Y)
R = np.sqrt(X**2 + Y**2)
Z = np.sin(R)

surf = ax.plot_surface(X, Y, Z, cmap = cm.coolwarm, linewidth = 0, antialiased = True)
ax.set_zlim(-1.01, 1.01)
ax.zaxis.set_major_locator(LinearLocator(10))
ax.zaxis.set_major_formatter(FormatStrFormatter('%.02f'))
fig.colorbar(surf, shrink = 0.5, aspect = 5)
#%%
fig = plt.figure(0, (6,5), 72)
ax = fig.gca(projection = '3d')
x = np.arange(0, 100, 0.25)
y = np.arange(0, 100, 0.25)
x, y = np.meshgrid(x, y)
x0 = 50
y0 = 50
sigma = 25.0                    # Standard deviation
f1 = 1.0 / (2 * np.pi * sigma**2) * np.e**(-((x-x0)**2 + (y-y0)**2) / (2 * sigma**2))

surf = ax.plot_wireframe(x, y, f1, rstride = 20, cstride = 20, antialiased = True)
