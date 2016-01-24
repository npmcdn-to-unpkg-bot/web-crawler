/// <binding Clean='clean' />
'use strict';

var gulp = require('gulp');
var react = require('gulp-react');
var rimraf = require('rimraf');
var concat = require('gulp-concat');
var	browserify = require('browserify');
var reactify = require('reactify');
var babelify = require('babelify');
var	source = require('vinyl-source-stream');
var cssmin = require('gulp-cssmin');
var uglify = require('gulp-uglify');
var data = '',
  dua = 2;
var paths = {
	webroot: './wwwroot/',
};

paths.lib = paths.webroot + 'lib';
paths.mainjs = paths.webroot + 'components/main.jsx';
paths.jsBuild = paths.webroot + 'js';
paths.js = paths.webroot + 'js/**/*.js';
paths.minJs = paths.webroot + 'js/**/*.min.js';
paths.jsx = paths.webroot + 'components/**/*.jsx';
paths.css = paths.webroot + 'css/**/*.css';
paths.minCss = paths.webroot + 'css/**/*.min.css';
paths.concatJsDest = paths.webroot + 'js/site.min.js';
paths.jsxDest = paths.webroot + 'components/**/*.js';
paths.concatJsxDest = paths.webroot + 'js/component.js';
paths.concatCssDest = paths.webroot + 'css/site.min.css';

gulp.task('clean:js', function(cb) {
	rimraf(paths.concatJsDest, cb);
});

gulp.task('clean:jsx', function(cb) {
	rimraf(paths.jsxDest, cb);
});

gulp.task('clean:css', function (cb) {
	rimraf(paths.concatCssDest, cb);
});

gulp.task('clean', ['clean:js', 'clean:jsx', 'clean:css']);

gulp.task('transform', function () {
	return browserify({
			extensions: ['.js', '.jsx'],
			entries: paths.mainjs,
			paths: [paths.lib],
			fullPaths: true
		})
		.transform(babelify)
		.on('prebundle', function(bundler) {
			bundler.require('react');
		})
		.bundle()
		.on('error', console.error.bind(console))
		.pipe(source('component.js'))
		.pipe(gulp.dest(paths.jsBuild));
});

gulp.task('build:jsx', function () {
	return gulp.src([paths.jsx, '!' + paths.jsxDest], { base: '.' })
		.pipe(react())
		.pipe(concat(paths.concatJsxDest))
		.pipe(uglify())
		.pipe(gulp.dest(paths.jsBuild));
});

gulp.task('min:js', function () {
	return gulp.src([paths.js, '!' + paths.minJs], { base: '.' })
        .pipe(concat(paths.concatJsDest))
        .pipe(uglify())
        .pipe(gulp.dest('.'));
});

gulp.task('min:css', function () {
	return gulp.src([paths.css, '!' + paths.minCss])
        .pipe(concat(paths.concatCssDest))
        .pipe(cssmin())
        .pipe(gulp.dest('.'));
});

gulp.task('min', ['transform', 'min:js', 'min:css']);
gulp.task('default', ['transform', 'min:js', 'min:css']);
