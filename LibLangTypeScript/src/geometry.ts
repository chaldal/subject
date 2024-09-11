export type Pixels = number;
export type PixelsPerMS = number;

export interface Coords {
	x: number;
	y: number;
}

export interface Size {
	w: number;
	h: number;
}

export interface BoundingBox {
	topLeft: Coords;
	size:    Size;
}

export enum Side {
	LEFT,
	RIGHT,
}
