/* eslint-disable */
/* tslint:disable */
// @ts-nocheck
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

export enum GrocerySort {
  Name = "Name",
  Price = "Price",
  Stock = "Stock",
  Rating = "Rating",
  Created = "Created",
}

export enum Supplier {
  LocalFarm = "LocalFarm",
  Wholesale = "Wholesale",
  Import = "Import",
}

export enum StorageType {
  Ambient = "Ambient",
  Chilled = "Chilled",
  Frozen = "Frozen",
}

export interface GroceryItem {
  /** @format guid */
  id: string;
  name: string;
  brand?: string | null;
  category: string;
  tags?: string | null;
  barcode?: string | null;
  /** @format decimal */
  priceDkk: number;
  /** @format decimal */
  discountPercent?: number | null;
  /**
   * @format int32
   * @min 1
   * @max 2147483647
   */
  stockCount: number;
  /** @format int32 */
  timesPurchased: number;
  /** @format double */
  weightKg: number;
  /** @format double */
  ratingAvg?: number | null;
  isOrganic: boolean;
  isDiscontinued: boolean;
  storage: StorageType;
  suppliedBy: Supplier;
  /** @format date-time */
  createdAtUtc: string;
  /** @format date-time */
  lastPurchasedAtUtc?: string | null;
  /** @format date */
  bestBefore?: string | null;
  /** @format duration */
  preparationTime?: string | null;
}

export interface MyAwesomeCrudCreateGroceryItemParams {
  name?: string;
  category?: string;
  branc?: string;
  /** @format date-time */
  createdAt?: string;
}

export interface MyAwesomeCrudUpdateThingParams {
  /** @format guid */
  id?: string;
  /** @format decimal */
  newDiscount?: number;
}

export interface MyAwesomeCrudDeleteThingParams {
  /** @format guid */
  id?: string;
}

export interface GroceriesDiscontinueParams {
  /** @format guid */
  id?: string;
}

export interface GroceriesReactivateParams {
  /** @format guid */
  id?: string;
}

export interface GroceriesRestockParams {
  category?: string;
  /** @format int32 */
  amount?: number;
}

export interface GroceriesClearDiscountsParams {
  category?: string;
}

export interface GroceriesApplyDiscountParams {
  category?: string;
  /** @format decimal */
  percent?: number;
}

export interface GroceriesDeleteParams {
  /** @format guid */
  id?: string;
}

export interface GroceriesPurchaseParams {
  /** @format guid */
  id?: string;
  /**
   * @format int32
   * @default 1
   */
  quantity?: number;
}

export interface GroceriesTransferStockParams {
  /** @format guid */
  fromId?: string;
  /** @format guid */
  toId?: string;
  /** @format int32 */
  amount?: number;
}

export interface GroceriesGetByStorageParams {
  storage?: StorageType;
}

export interface GroceriesGetByCategoryParams {
  category?: string;
}

export interface GroceriesGetByIdParams {
  /** @format guid */
  id?: string;
}

export interface GroceriesExistsParams {
  barcode?: string;
}

export interface GroceriesGetByBarcodeParams {
  barcode?: string;
}

export interface GroceriesGetByPriceRangeParams {
  /** @format decimal */
  min?: number | null;
  /** @format decimal */
  max?: number | null;
}

export interface GroceriesSearchParams {
  q?: string;
}

export interface GroceriesCountInCategoryParams {
  category?: string;
}

export interface GroceriesCountByStorageParams {
  storage?: StorageType;
}

export interface GroceriesGetTopPurchasedParams {
  /**
   * @format int32
   * @default 5
   */
  n?: number;
}

export interface GroceriesGetPageParams {
  /**
   * @format int32
   * @default 1
   */
  page?: number;
  /**
   * @format int32
   * @default 10
   */
  size?: number;
}

export interface GroceriesGetSortedParams {
  by?: GrocerySort;
  /** @default false */
  descending?: boolean;
}

export interface GroceriesGetAveragePriceInCategoryParams {
  category?: string;
}

export interface GroceriesGetExpiringParams {
  /**
   * @format int32
   * @default 7
   */
  days?: number;
}

export interface GroceriesGetRecentlyAddedParams {
  /**
   * @format int32
   * @default 180
   */
  days?: number;
}

export interface GroceriesGetStaleParams {
  /**
   * @format int32
   * @default 30
   */
  days?: number;
}

export interface GroceriesGetQuickToPrepareParams {
  /**
   * @format int32
   * @default 15
   */
  maxMinutes?: number;
}

export interface GroceriesGetByTagParams {
  tag?: string;
}

export interface GroceriesGetPriceAfterDiscountParams {
  /** @format guid */
  id?: string;
}

export interface GroceriesGetFilteredParams {
  q?: string | null;
  category?: string | null;
  storage?: StorageType | null;
  isOrganic?: boolean | null;
  inStock?: boolean | null;
  /** @format decimal */
  minPrice?: number | null;
  /** @format decimal */
  maxPrice?: number | null;
}

export type QueryParamsType = Record<string | number, any>;
export type ResponseFormat = keyof Omit<Body, "body" | "bodyUsed">;

export interface FullRequestParams extends Omit<RequestInit, "body"> {
  /** set parameter to `true` for call `securityWorker` for this request */
  secure?: boolean;
  /** request path */
  path: string;
  /** content type of request body */
  type?: ContentType;
  /** query params */
  query?: QueryParamsType;
  /** format of response (i.e. response.json() -> format: "json") */
  format?: ResponseFormat;
  /** request body */
  body?: unknown;
  /** base url */
  baseUrl?: string;
  /** request cancellation token */
  cancelToken?: CancelToken;
}

export type RequestParams = Omit<
  FullRequestParams,
  "body" | "method" | "query" | "path"
>;

export interface ApiConfig<SecurityDataType = unknown> {
  baseUrl?: string;
  baseApiParams?: Omit<RequestParams, "baseUrl" | "cancelToken" | "signal">;
  securityWorker?: (
    securityData: SecurityDataType | null,
  ) => Promise<RequestParams | void> | RequestParams | void;
  customFetch?: typeof fetch;
}

export interface HttpResponse<D extends unknown, E extends unknown = unknown>
  extends Response {
  data: D;
  error: E;
}

type CancelToken = Symbol | string | number;

export enum ContentType {
  Json = "application/json",
  JsonApi = "application/vnd.api+json",
  FormData = "multipart/form-data",
  UrlEncoded = "application/x-www-form-urlencoded",
  Text = "text/plain",
}

export class HttpClient<SecurityDataType = unknown> {
  public baseUrl: string = "http://localhost:5234";
  private securityData: SecurityDataType | null = null;
  private securityWorker?: ApiConfig<SecurityDataType>["securityWorker"];
  private abortControllers = new Map<CancelToken, AbortController>();
  private customFetch = (...fetchParams: Parameters<typeof fetch>) =>
    fetch(...fetchParams);

  private baseApiParams: RequestParams = {
    credentials: "same-origin",
    headers: {},
    redirect: "follow",
    referrerPolicy: "no-referrer",
  };

  constructor(apiConfig: ApiConfig<SecurityDataType> = {}) {
    Object.assign(this, apiConfig);
  }

  public setSecurityData = (data: SecurityDataType | null) => {
    this.securityData = data;
  };

  protected encodeQueryParam(key: string, value: any) {
    const encodedKey = encodeURIComponent(key);
    return `${encodedKey}=${encodeURIComponent(typeof value === "number" ? value : `${value}`)}`;
  }

  protected addQueryParam(query: QueryParamsType, key: string) {
    return this.encodeQueryParam(key, query[key]);
  }

  protected addArrayQueryParam(query: QueryParamsType, key: string) {
    const value = query[key];
    return value.map((v: any) => this.encodeQueryParam(key, v)).join("&");
  }

  protected toQueryString(rawQuery?: QueryParamsType): string {
    const query = rawQuery || {};
    const keys = Object.keys(query).filter(
      (key) => "undefined" !== typeof query[key],
    );
    return keys
      .map((key) =>
        Array.isArray(query[key])
          ? this.addArrayQueryParam(query, key)
          : this.addQueryParam(query, key),
      )
      .join("&");
  }

  protected addQueryParams(rawQuery?: QueryParamsType): string {
    const queryString = this.toQueryString(rawQuery);
    return queryString ? `?${queryString}` : "";
  }

  private contentFormatters: Record<ContentType, (input: any) => any> = {
    [ContentType.Json]: (input: any) =>
      input !== null && (typeof input === "object" || typeof input === "string")
        ? JSON.stringify(input)
        : input,
    [ContentType.JsonApi]: (input: any) =>
      input !== null && (typeof input === "object" || typeof input === "string")
        ? JSON.stringify(input)
        : input,
    [ContentType.Text]: (input: any) =>
      input !== null && typeof input !== "string"
        ? JSON.stringify(input)
        : input,
    [ContentType.FormData]: (input: any) => {
      if (input instanceof FormData) {
        return input;
      }

      return Object.keys(input || {}).reduce((formData, key) => {
        const property = input[key];
        formData.append(
          key,
          property instanceof Blob
            ? property
            : typeof property === "object" && property !== null
              ? JSON.stringify(property)
              : `${property}`,
        );
        return formData;
      }, new FormData());
    },
    [ContentType.UrlEncoded]: (input: any) => this.toQueryString(input),
  };

  protected mergeRequestParams(
    params1: RequestParams,
    params2?: RequestParams,
  ): RequestParams {
    return {
      ...this.baseApiParams,
      ...params1,
      ...(params2 || {}),
      headers: {
        ...(this.baseApiParams.headers || {}),
        ...(params1.headers || {}),
        ...((params2 && params2.headers) || {}),
      },
    };
  }

  protected createAbortSignal = (
    cancelToken: CancelToken,
  ): AbortSignal | undefined => {
    if (this.abortControllers.has(cancelToken)) {
      const abortController = this.abortControllers.get(cancelToken);
      if (abortController) {
        return abortController.signal;
      }
      return void 0;
    }

    const abortController = new AbortController();
    this.abortControllers.set(cancelToken, abortController);
    return abortController.signal;
  };

  public abortRequest = (cancelToken: CancelToken) => {
    const abortController = this.abortControllers.get(cancelToken);

    if (abortController) {
      abortController.abort();
      this.abortControllers.delete(cancelToken);
    }
  };

  public request = async <T = any, E = any>({
    body,
    secure,
    path,
    type,
    query,
    format,
    baseUrl,
    cancelToken,
    ...params
  }: FullRequestParams): Promise<T> => {
    const secureParams =
      ((typeof secure === "boolean" ? secure : this.baseApiParams.secure) &&
        this.securityWorker &&
        (await this.securityWorker(this.securityData))) ||
      {};
    const requestParams = this.mergeRequestParams(params, secureParams);
    const queryString = query && this.toQueryString(query);
    const payloadFormatter = this.contentFormatters[type || ContentType.Json];
    const responseFormat = format || requestParams.format;

    return this.customFetch(
      `${baseUrl || this.baseUrl || ""}${path}${queryString ? `?${queryString}` : ""}`,
      {
        ...requestParams,
        headers: {
          ...(requestParams.headers || {}),
          ...(type && type !== ContentType.FormData
            ? { "Content-Type": type }
            : {}),
        },
        signal:
          (cancelToken
            ? this.createAbortSignal(cancelToken)
            : requestParams.signal) || null,
        body:
          typeof body === "undefined" || body === null
            ? null
            : payloadFormatter(body),
      },
    ).then(async (response) => {
      const r = response as HttpResponse<T, E>;
      r.data = null as unknown as T;
      r.error = null as unknown as E;

      const responseToParse = responseFormat ? response.clone() : response;
      const data = !responseFormat
        ? r
        : await responseToParse[responseFormat]()
            .then((data) => {
              if (r.ok) {
                r.data = data;
              } else {
                r.error = data;
              }
              return r;
            })
            .catch((e) => {
              r.error = e;
              return r;
            });

      if (cancelToken) {
        this.abortControllers.delete(cancelToken);
      }

      if (!response.ok) throw data;
      return data.data;
    });
  };
}

/**
 * @title My Title
 * @version 1.0.0
 * @baseUrl http://localhost:5234
 */
export class Api<
  SecurityDataType extends unknown,
> extends HttpClient<SecurityDataType> {
  createGroceryItem = {
    /**
     * No description
     *
     * @tags MyAwesomeCrud
     * @name MyAwesomeCrudCreateGroceryItem
     * @request POST:/CreateGroceryItem
     */
    myAwesomeCrudCreateGroceryItem: (
      query: MyAwesomeCrudCreateGroceryItemParams = {},
      params: RequestParams = {},
    ) =>
      this.request<number, any>({
        path: `/CreateGroceryItem`,
        method: "POST",
        query: query,
        format: "json",
        ...params,
      }),
  };
  updateThing = {
    /**
     * No description
     *
     * @tags MyAwesomeCrud
     * @name MyAwesomeCrudUpdateThing
     * @request PUT:/UpdateThing
     */
    myAwesomeCrudUpdateThing: (
      query: MyAwesomeCrudUpdateThingParams = {},
      params: RequestParams = {},
    ) =>
      this.request<GroceryItem, any>({
        path: `/UpdateThing`,
        method: "PUT",
        query: query,
        format: "json",
        ...params,
      }),
  };
  deleteThing = {
    /**
     * No description
     *
     * @tags MyAwesomeCrud
     * @name MyAwesomeCrudDeleteThing
     * @request DELETE:/DeleteThing
     */
    myAwesomeCrudDeleteThing: (
      query: MyAwesomeCrudDeleteThingParams = {},
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/DeleteThing`,
        method: "DELETE",
        query: query,
        ...params,
      }),
  };
  discontinue = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesDiscontinue
     * @request POST:/Discontinue
     */
    groceriesDiscontinue: (
      query: GroceriesDiscontinueParams = {},
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/Discontinue`,
        method: "POST",
        query: query,
        ...params,
      }),
  };
  reactivate = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesReactivate
     * @request PATCH:/Reactivate
     */
    groceriesReactivate: (
      query: GroceriesReactivateParams = {},
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/Reactivate`,
        method: "PATCH",
        query: query,
        ...params,
      }),
  };
  restock = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesRestock
     * @request POST:/Restock
     */
    groceriesRestock: (
      query: GroceriesRestockParams = {},
      params: RequestParams = {},
    ) =>
      this.request<number, any>({
        path: `/Restock`,
        method: "POST",
        query: query,
        format: "json",
        ...params,
      }),
  };
  clearDiscounts = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesClearDiscounts
     * @request POST:/ClearDiscounts
     */
    groceriesClearDiscounts: (
      query: GroceriesClearDiscountsParams = {},
      params: RequestParams = {},
    ) =>
      this.request<number, any>({
        path: `/ClearDiscounts`,
        method: "POST",
        query: query,
        format: "json",
        ...params,
      }),
  };
  applyDiscount = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesApplyDiscount
     * @request POST:/ApplyDiscount
     */
    groceriesApplyDiscount: (
      query: GroceriesApplyDiscountParams = {},
      params: RequestParams = {},
    ) =>
      this.request<number, any>({
        path: `/ApplyDiscount`,
        method: "POST",
        query: query,
        format: "json",
        ...params,
      }),
  };
  deleteExpired = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesDeleteExpired
     * @request DELETE:/DeleteExpired
     */
    groceriesDeleteExpired: (params: RequestParams = {}) =>
      this.request<number, any>({
        path: `/DeleteExpired`,
        method: "DELETE",
        format: "json",
        ...params,
      }),
  };
  delete = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesDelete
     * @request DELETE:/Delete
     */
    groceriesDelete: (
      query: GroceriesDeleteParams = {},
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/Delete`,
        method: "DELETE",
        query: query,
        ...params,
      }),
  };
  create = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesCreate
     * @request POST:/Create
     */
    groceriesCreate: (data: GroceryItem, params: RequestParams = {}) =>
      this.request<string, any>({
        path: `/Create`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),
  };
  purchase = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesPurchase
     * @request POST:/Purchase
     */
    groceriesPurchase: (
      query: GroceriesPurchaseParams = {},
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/Purchase`,
        method: "POST",
        query: query,
        ...params,
      }),
  };
  update = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesUpdate
     * @request PUT:/Update
     */
    groceriesUpdate: (data: GroceryItem, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/Update`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        ...params,
      }),
  };
  upsert = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesUpsert
     * @request PUT:/Upsert
     */
    groceriesUpsert: (data: GroceryItem, params: RequestParams = {}) =>
      this.request<string, any>({
        path: `/Upsert`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),
  };
  import = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesImport
     * @request POST:/Import
     */
    groceriesImport: (data: GroceryItem[], params: RequestParams = {}) =>
      this.request<number, any>({
        path: `/Import`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),
  };
  transferStock = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesTransferStock
     * @request POST:/TransferStock
     */
    groceriesTransferStock: (
      query: GroceriesTransferStockParams = {},
      params: RequestParams = {},
    ) =>
      this.request<void, any>({
        path: `/TransferStock`,
        method: "POST",
        query: query,
        ...params,
      }),
  };
  getAllMyGroceries = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesGetAllMyGroceries
     * @request GET:/GetAllMyGroceries
     */
    groceriesGetAllMyGroceries: (params: RequestParams = {}) =>
      this.request<GroceryItem[], any>({
        path: `/GetAllMyGroceries`,
        method: "GET",
        format: "json",
        ...params,
      }),
  };
  count = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesCount
     * @request GET:/Count
     */
    groceriesCount: (params: RequestParams = {}) =>
      this.request<number, any>({
        path: `/Count`,
        method: "GET",
        format: "json",
        ...params,
      }),
  };
  getOrganic = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesGetOrganic
     * @request GET:/GetOrganic
     */
    groceriesGetOrganic: (params: RequestParams = {}) =>
      this.request<GroceryItem[], any>({
        path: `/GetOrganic`,
        method: "GET",
        format: "json",
        ...params,
      }),
  };
  getByStorage = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesGetByStorage
     * @request GET:/GetByStorage
     */
    groceriesGetByStorage: (
      query: GroceriesGetByStorageParams = {},
      params: RequestParams = {},
    ) =>
      this.request<GroceryItem[], any>({
        path: `/GetByStorage`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),
  };
  getOutOfStock = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesGetOutOfStock
     * @request GET:/GetOutOfStock
     */
    groceriesGetOutOfStock: (params: RequestParams = {}) =>
      this.request<GroceryItem[], any>({
        path: `/GetOutOfStock`,
        method: "GET",
        format: "json",
        ...params,
      }),
  };
  getIncomplete = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesGetIncomplete
     * @request GET:/GetIncomplete
     */
    groceriesGetIncomplete: (params: RequestParams = {}) =>
      this.request<GroceryItem[], any>({
        path: `/GetIncomplete`,
        method: "GET",
        format: "json",
        ...params,
      }),
  };
  getDiscounted = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesGetDiscounted
     * @request GET:/GetDiscounted
     */
    groceriesGetDiscounted: (params: RequestParams = {}) =>
      this.request<GroceryItem[], any>({
        path: `/GetDiscounted`,
        method: "GET",
        format: "json",
        ...params,
      }),
  };
  getByCategory = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesGetByCategory
     * @request GET:/GetByCategory
     */
    groceriesGetByCategory: (
      query: GroceriesGetByCategoryParams = {},
      params: RequestParams = {},
    ) =>
      this.request<GroceryItem[], any>({
        path: `/GetByCategory`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),
  };
  getById = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesGetById
     * @request GET:/GetById
     */
    groceriesGetById: (
      query: GroceriesGetByIdParams = {},
      params: RequestParams = {},
    ) =>
      this.request<GroceryItem, any>({
        path: `/GetById`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),
  };
  exists = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesExists
     * @request GET:/Exists
     */
    groceriesExists: (
      query: GroceriesExistsParams = {},
      params: RequestParams = {},
    ) =>
      this.request<boolean, any>({
        path: `/Exists`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),
  };
  getByBarcode = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesGetByBarcode
     * @request GET:/GetByBarcode
     */
    groceriesGetByBarcode: (
      query: GroceriesGetByBarcodeParams = {},
      params: RequestParams = {},
    ) =>
      this.request<GroceryItem, any>({
        path: `/GetByBarcode`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),
  };
  getByPriceRange = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesGetByPriceRange
     * @request GET:/GetByPriceRange
     */
    groceriesGetByPriceRange: (
      query: GroceriesGetByPriceRangeParams = {},
      params: RequestParams = {},
    ) =>
      this.request<GroceryItem[], any>({
        path: `/GetByPriceRange`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),
  };
  search = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesSearch
     * @request GET:/Search
     */
    groceriesSearch: (
      query: GroceriesSearchParams = {},
      params: RequestParams = {},
    ) =>
      this.request<GroceryItem[], any>({
        path: `/Search`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),
  };
  getCategories = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesGetCategories
     * @request GET:/GetCategories
     */
    groceriesGetCategories: (params: RequestParams = {}) =>
      this.request<string[], any>({
        path: `/GetCategories`,
        method: "GET",
        format: "json",
        ...params,
      }),
  };
  countInCategory = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesCountInCategory
     * @request GET:/CountInCategory
     */
    groceriesCountInCategory: (
      query: GroceriesCountInCategoryParams = {},
      params: RequestParams = {},
    ) =>
      this.request<number, any>({
        path: `/CountInCategory`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),
  };
  countByStorage = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesCountByStorage
     * @request GET:/CountByStorage
     */
    groceriesCountByStorage: (
      query: GroceriesCountByStorageParams = {},
      params: RequestParams = {},
    ) =>
      this.request<number, any>({
        path: `/CountByStorage`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),
  };
  getTopPurchased = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesGetTopPurchased
     * @request GET:/GetTopPurchased
     */
    groceriesGetTopPurchased: (
      query: GroceriesGetTopPurchasedParams = {},
      params: RequestParams = {},
    ) =>
      this.request<GroceryItem[], any>({
        path: `/GetTopPurchased`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),
  };
  getPage = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesGetPage
     * @request GET:/GetPage
     */
    groceriesGetPage: (
      query: GroceriesGetPageParams = {},
      params: RequestParams = {},
    ) =>
      this.request<GroceryItem[], any>({
        path: `/GetPage`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),
  };
  getSorted = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesGetSorted
     * @request GET:/GetSorted
     */
    groceriesGetSorted: (
      query: GroceriesGetSortedParams = {},
      params: RequestParams = {},
    ) =>
      this.request<GroceryItem[], any>({
        path: `/GetSorted`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),
  };
  getAveragePrice = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesGetAveragePrice
     * @request GET:/GetAveragePrice
     */
    groceriesGetAveragePrice: (params: RequestParams = {}) =>
      this.request<number, any>({
        path: `/GetAveragePrice`,
        method: "GET",
        format: "json",
        ...params,
      }),
  };
  getTotalStockValue = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesGetTotalStockValue
     * @request GET:/GetTotalStockValue
     */
    groceriesGetTotalStockValue: (params: RequestParams = {}) =>
      this.request<number, any>({
        path: `/GetTotalStockValue`,
        method: "GET",
        format: "json",
        ...params,
      }),
  };
  getAverageRating = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesGetAverageRating
     * @request GET:/GetAverageRating
     */
    groceriesGetAverageRating: (params: RequestParams = {}) =>
      this.request<number | null, any>({
        path: `/GetAverageRating`,
        method: "GET",
        format: "json",
        ...params,
      }),
  };
  getAveragePriceInCategory = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesGetAveragePriceInCategory
     * @request GET:/GetAveragePriceInCategory
     */
    groceriesGetAveragePriceInCategory: (
      query: GroceriesGetAveragePriceInCategoryParams = {},
      params: RequestParams = {},
    ) =>
      this.request<number, any>({
        path: `/GetAveragePriceInCategory`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),
  };
  getExpired = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesGetExpired
     * @request GET:/GetExpired
     */
    groceriesGetExpired: (params: RequestParams = {}) =>
      this.request<GroceryItem[], any>({
        path: `/GetExpired`,
        method: "GET",
        format: "json",
        ...params,
      }),
  };
  getExpiring = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesGetExpiring
     * @request GET:/GetExpiring
     */
    groceriesGetExpiring: (
      query: GroceriesGetExpiringParams = {},
      params: RequestParams = {},
    ) =>
      this.request<GroceryItem[], any>({
        path: `/GetExpiring`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),
  };
  getRecentlyAdded = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesGetRecentlyAdded
     * @request GET:/GetRecentlyAdded
     */
    groceriesGetRecentlyAdded: (
      query: GroceriesGetRecentlyAddedParams = {},
      params: RequestParams = {},
    ) =>
      this.request<GroceryItem[], any>({
        path: `/GetRecentlyAdded`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),
  };
  getStale = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesGetStale
     * @request GET:/GetStale
     */
    groceriesGetStale: (
      query: GroceriesGetStaleParams = {},
      params: RequestParams = {},
    ) =>
      this.request<GroceryItem[], any>({
        path: `/GetStale`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),
  };
  getQuickToPrepare = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesGetQuickToPrepare
     * @request GET:/GetQuickToPrepare
     */
    groceriesGetQuickToPrepare: (
      query: GroceriesGetQuickToPrepareParams = {},
      params: RequestParams = {},
    ) =>
      this.request<GroceryItem[], any>({
        path: `/GetQuickToPrepare`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),
  };
  getLatest = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesGetLatest
     * @request GET:/GetLatest
     */
    groceriesGetLatest: (params: RequestParams = {}) =>
      this.request<GroceryItem, any>({
        path: `/GetLatest`,
        method: "GET",
        format: "json",
        ...params,
      }),
  };
  getLastPurchased = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesGetLastPurchased
     * @request GET:/GetLastPurchased
     */
    groceriesGetLastPurchased: (params: RequestParams = {}) =>
      this.request<GroceryItem, any>({
        path: `/GetLastPurchased`,
        method: "GET",
        format: "json",
        ...params,
      }),
  };
  getByTag = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesGetByTag
     * @request GET:/GetByTag
     */
    groceriesGetByTag: (
      query: GroceriesGetByTagParams = {},
      params: RequestParams = {},
    ) =>
      this.request<GroceryItem[], any>({
        path: `/GetByTag`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),
  };
  getPriceAfterDiscount = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesGetPriceAfterDiscount
     * @request GET:/GetPriceAfterDiscount
     */
    groceriesGetPriceAfterDiscount: (
      query: GroceriesGetPriceAfterDiscountParams = {},
      params: RequestParams = {},
    ) =>
      this.request<number, any>({
        path: `/GetPriceAfterDiscount`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),
  };
  getFiltered = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesGetFiltered
     * @request GET:/GetFiltered
     */
    groceriesGetFiltered: (
      query: GroceriesGetFilteredParams = {},
      params: RequestParams = {},
    ) =>
      this.request<GroceryItem[], any>({
        path: `/GetFiltered`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),
  };
  getCheapestPerCategory = {
    /**
     * No description
     *
     * @tags Groceries
     * @name GroceriesGetCheapestPerCategory
     * @request GET:/GetCheapestPerCategory
     */
    groceriesGetCheapestPerCategory: (params: RequestParams = {}) =>
      this.request<GroceryItem[], any>({
        path: `/GetCheapestPerCategory`,
        method: "GET",
        format: "json",
        ...params,
      }),
  };
}
