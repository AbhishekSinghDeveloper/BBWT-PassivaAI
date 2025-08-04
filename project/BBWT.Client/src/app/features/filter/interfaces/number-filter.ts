import { ICountableBetweenFilterBase, ICountableFilterBase } from "./countable-filter-base";


export interface INumberFilter extends ICountableFilterBase<number> {}

export interface INumberBetweenFilter extends ICountableBetweenFilterBase<number> {}
