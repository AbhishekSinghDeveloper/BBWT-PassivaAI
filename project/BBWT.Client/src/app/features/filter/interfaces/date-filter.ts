import { ICountableBetweenFilterBase, ICountableFilterBase } from "./countable-filter-base";


export interface IDateFilter extends ICountableFilterBase<Date> { }

export interface IDateBetweenFilter extends ICountableBetweenFilterBase<Date> {}
