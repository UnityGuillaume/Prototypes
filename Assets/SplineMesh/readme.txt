Allow to duplicate & deform a mesh along a spline.

Add the Path mesh component to an object, set a mesh to be duplicated (will
duplicate along the Z axis) and add points/modify the spline to see it deformed
and duplicated.

TODO
----

- Better spline control
  - delete point
  - tangent follow the point they are associated with
  - can lock tangent in certain mode
- Better handling of world size (right now just splite the spline, so a short
 segement have the same ratio than a long one)
- Fix some mesh generation issue (normals seems wrong)
