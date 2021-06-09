//
//  UIImage+Watermark.swift
//  Checklyst
//
//  Created by Matematici Developer on 31/05/2021.
//  Copyright © 2021 Johnson&Johnson. All rights reserved.
//

import Foundation
import UIKit

extension UIImage {
    
    public enum WatermarkPosition {
        case Center, LeftTop, LeftBottom, RightTop, RightBottom
    }
    
    func addWatermark(position: WatermarkPosition, watermark: UIImage, margin: CGFloat?) -> UIImage {
        let renderer = UIGraphicsImageRenderer(size: CGSize(width: self.size.width, height: self.size.height))
        
        let image = renderer.image { ctx in
            self.draw(at: CGPoint(x: 0, y: 0))
            let rect = getRectForPosition(position, imageSize: self.size, waterMarkSize: watermark.size, margin: margin)
            watermark.draw(in: rect, blendMode: .normal, alpha: 0.8)
        }
        return image
    }
    
    private func getRectForPosition(_ position: WatermarkPosition, imageSize: CGSize, waterMarkSize: CGSize, margin: CGFloat?) -> CGRect {
        let imageHeight = imageSize.height
        let imageWidth = imageSize.width
        let watermarkHeight = waterMarkSize.height
        let watermarkWidth = waterMarkSize.width
        switch position {
        case .Center:
            return CGRect(x: imageWidth - (watermarkWidth * 1.5) , y: imageHeight - (watermarkHeight * 1.5), width: watermarkWidth, height: watermarkHeight)
        case .LeftTop:
            return CGRect(x: watermarkWidth + (margin ?? 0.0) , y: watermarkHeight + (margin ?? 0.0), width: watermarkWidth, height: watermarkHeight)
        case .LeftBottom:
            return CGRect(x: watermarkWidth + (margin ?? 0.0) , y: imageHeight - watermarkHeight - (margin ?? 0.0), width: watermarkWidth, height: watermarkHeight)
        case .RightTop:
            return CGRect(x: imageWidth - watermarkWidth - (margin ?? 0.0) , y: watermarkHeight + (margin ?? 0.0), width: watermarkWidth, height: watermarkHeight)
        case .RightBottom:
            return CGRect(x: imageWidth - watermarkWidth - (margin ?? 0.0) , y: imageHeight - watermarkHeight - (margin ?? 0.0), width: watermarkWidth, height: watermarkHeight)
        }
    }
}
