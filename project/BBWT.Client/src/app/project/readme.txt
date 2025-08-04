1) Add all project-specific modules inside this folder.
2) It's structure should follow the same rules as for app/main/ folder:

    app/project/feature1
        * feature 1 module files *
        feature1.module.ts

    app/project/feature2
        * feature 2 module files *
        feature2.module.ts

    ...

3) Define all project-specific modules routing inside project-routing.ts file
   similar to /app/main/main-routing.module.ts.