//
//  QRScannerViewDelegate.swift
//  Checklyst
//
//  Created by Simone Testa on 29/07/2019.
//  Copyright © 2019 Johnson&Johnson. All rights reserved.
//

import Foundation

protocol QRScannerViewDelegate: class {
    func qrScanningDidFail()
    func qrScanningSucceededWithCode(_ str: String?)
    func qrScanningDidStop()
}
