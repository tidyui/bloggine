var gulp = require('gulp'),
    sass = require('gulp-sass')
    cssnano = require('gulp-cssnano')
    rename = require("gulp-rename");

gulp.task('min', function (done) {
    gulp.src('src/scss/style.scss')
        .pipe(sass().on('error', sass.logError))
        .pipe(cssnano())
        .pipe(rename({
            suffix: ".min"
        }))
        .pipe(gulp.dest('Assets/css'));
    done();
});

gulp.task("serve", gulp.parallel(["min"]));
gulp.task("default", gulp.series("serve"));
