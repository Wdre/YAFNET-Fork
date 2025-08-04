﻿import '@popperjs/core';
import 'bootstrap';

import Lightbox from '@w8tcha/bs5-lightbox';

// Custom JS imports
import DarkEditable from '@w8tcha/dark-editable';

if (document.querySelector('.album-caption')) {
	document.querySelectorAll<HTMLElement>('.album-caption').forEach(el => {
		const _ = new DarkEditable(el);
	});
}

if (document.querySelector('.album-image-caption')) {
	document.querySelectorAll<HTMLElement>('.album-image-caption').forEach(el => {
		const _ = new DarkEditable(el);
	});
}

document.addEventListener('DOMContentLoaded', () => {
	// Gallery
	document.querySelectorAll<HTMLElement>('[data-toggle="lightbox"]').forEach(element => {
		element.addEventListener('click', Lightbox.initialize);
	});
});