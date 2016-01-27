/// <binding Clean='clean' />
'use strict';

var gulp = require('gulp');
var clone = require('gulp-clone');
var sourcemaps = require('gulp-sourcemaps');
var connect = require('gulp-connect');
var open = require('gulp-open');
var babel = require('gulp-babel');
var react = require('gulp-react');
var rimraf = require('rimraf');
var concat = require('gulp-concat');
var	browserify = require('browserify');
var reactify = require('reactify');
var babelify = require('babelify');
var	source = require('vinyl-source-stream');
var buffer = require('vinyl-buffer');
var cssmin = require('gulp-cssmin');
var uglify = require('gulp-uglify');
var nodeResolve = require('resolve');
var fs = require('fs');

var paths = {
	webroot: './wwwroot/',
};

var config = {
	port: 8080,
	baseUrl: 'http://localhost',
	defaultFile: 'index.html'
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
paths.html = paths.webroot + '*.html';

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

var cloneSink = clone.sink();
gulp.task('transform', function () {
	return browserify({
			extensions: ['.js', '.jsx'],
			entries: paths.mainjs,
			paths: [paths.lib]
		})
		.transform(babelify, { presets: ['es2015', 'react']})
		// .require(nodeResolve.sync('react'), { expose: 'react' })
		// .require(nodeResolve.sync('react-dom'), { expose: 'react-dom' })
		.bundle()
		.on('error', console.error.bind(console))
		.pipe(source('component.js'))
		.pipe(buffer())
		.pipe(cloneSink)
		.pipe(concat('component.min.js'))
		.pipe(uglify())
		.pipe(cloneSink.tap())
		.pipe(gulp.dest(paths.jsBuild))
		.pipe(connect.reload());
});

gulp.task('jsx:babel', function () {
	return gulp.src(paths.jsx)
		.pipe(sourcemaps.init())
		.pipe(babel())
		.pipe(concat(paths.concatJsxDest))
		.pipe(sourcemaps.write('.'))
		.pipe(buffer())
		// .pipe(cloneSink)
		.pipe(uglify())
		// .pipe(cloneSink.tap())
		.pipe(gulp.dest(paths.jsBuild))
		.pipe(connect.reload()); 
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
        .pipe(gulp.dest('.'))
		.pipe(connect.reload());
});

gulp.task('connect', function () {
	connect.server({
		root: [paths.webroot],
		port: config.port,
		base: config.baseUrl,
		livereload: true
	});
});

gulp.task('open', ['connect'], function () {
	gulp.src(paths.webroot + config.defaultFile)
		.pipe(open({ uri: config.baseUrl + ':' + + config.port + '/' + config.defaultFile }));
});

gulp.task('html', function () {
	gulp.src(paths.html)
		.pipe(connect.reload());
});

gulp.task('watch', function () {
	gulp.watch(paths.html, ['html']);
	gulp.watch(paths.css, ['min:css']);
	gulp.watch(paths.jsx, ['jsx:babel']);
	gulp.watch(paths.jsx, ['jsx:babel']);
})

gulp.task('min', ['transform', 'min:js', 'min:css']);
// gulp.task('default', ['jsx:babel', 'min:js', 'min:css']);
gulp.task('default', ['min:css', 'transform', 'min:js', 'html', 'open', 'watch']);

