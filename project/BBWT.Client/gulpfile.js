const gulp = require('gulp');
const sass = require('gulp-sass')(require('sass'));
const sassGlob = require('gulp-sass-glob');
const merge = require('merge-stream');

gulp.task('themes', function () {
    return merge(
        gulp
            .src('src/assets/themes/ultima/layout/sass/*.scss')
            .pipe(sassGlob())
            .pipe(sass())
            .pipe(gulp.dest('src/assets/themes/output/ultima')),
        gulp
            .src('src/assets/themes/ultima/theme/*.scss')
            .pipe(sassGlob())
            .pipe(sass())
            .pipe(gulp.dest('src/assets/themes/output/ultima')),
        gulp
            .src('src/assets/themes/ultima/layout/fonts/*')
            .pipe(gulp.dest('src/assets/themes/output/ultima')),
        gulp
            .src('src/assets/themes/verona/layout/sass/*.scss')
            .pipe(sassGlob())
            .pipe(sass())
            .pipe(gulp.dest('src/assets/themes/output/verona')),
        gulp
            .src('src/assets/themes/verona/theme/*.scss')
            .pipe(sassGlob())
            .pipe(sass())
            .pipe(gulp.dest('src/assets/themes/output/verona')),
        gulp
            .src('src/assets/themes/verona/layout/fonts/*')
            .pipe(gulp.dest('src/assets/themes/output/verona'))
    );
});